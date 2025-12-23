using AlmaHogarFront.DTO;
using AlmaHogarFront.Models;
using AlmaHogarFront.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AlmaHogarFront.Controllers
{
    public class VentaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPromocionService _promocionService;
        private readonly ILogger<VentaController> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
       
        public VentaController(
            IHttpClientFactory httpClientFactory, 
            IPromocionService promocionService,
            ILogger<VentaController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _promocionService = promocionService;
            _logger = logger;

            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }

        //metodos para session get set

        private List<ProductoVM> GetProductosSession()
        {
            var json = HttpContext.Session.GetString("Productos");
            return json == null? new List<ProductoVM>(): JsonConvert.DeserializeObject<List<ProductoVM>>(json);
        }

        private void SetProductosSession(List<ProductoVM> productos)
        {
            HttpContext.Session.SetString("Productos", JsonConvert.SerializeObject(productos));
        }

        private List<PromocionDTO> GetPromosSession()
        {
            var json = HttpContext.Session.GetString("Promos");
            return json == null? new List<PromocionDTO>(): JsonConvert.DeserializeObject<List<PromocionDTO>>(json);
        }

        private void SetPromosSession(List<PromocionDTO> promos)
        {
            HttpContext.Session.SetString("Promos",JsonConvert.SerializeObject(promos));
        }

        private double GetSubTotal()
        {
            return HttpContext.Session.GetString("SubTotal") == null? 0: 
                JsonConvert.DeserializeObject<double>(HttpContext.Session.GetString("SubTotal"));
        }

        private void SetSubTotal(double value)
        {
            HttpContext.Session.SetString("SubTotal",JsonConvert.SerializeObject(value));
        }

        private double GetDescuento()
        {
            return HttpContext.Session.GetString("Descuento") == null? 0: JsonConvert.DeserializeObject<double>(
                    HttpContext.Session.GetString("Descuento"));
        }

        private void SetDescuento(double value)
        {
            HttpContext.Session.SetString("Descuento",JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// Obtiene las promociones aplicadas usando el servicio dedicado
        /// </summary>
        private async Task<List<PromocionDTO>> ObtenerPromocionesAplicadas(List<ProductoCompraDTO> productos)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no disponible en sesión");
                    return new List<PromocionDTO>();
                }

                var promociones = await _promocionService.ObtenerPromocionesAplicadas(productos, token);
                return promociones.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error obteniendo promociones aplicadas: {ex.Message}");
                return new List<PromocionDTO>();
            }
        }

        public async Task<IActionResult> Index(string nombre)
        {
            try
            {
                var productosSession = GetProductosSession();
                _logger.LogInformation($"Productos en sesión: {productosSession.Count}");
         
                ViewBag.ListaProd = productosSession;
        
                if (productosSession.Any())
                {
                    var prodCompraList = productosSession.Select(item => new ProductoCompraDTO
                    {
                        idProducto = item.codigo,
                        cantidad = item.cantidad
                    }).ToList();

                    // Llamar al servicio para obtener promociones
                    var promocionesAplicadas = await ObtenerPromocionesAplicadas(prodCompraList);

                    if (promocionesAplicadas.Any())
                    {
                        SetPromosSession(promocionesAplicadas);
                        var descuentoTotal = promocionesAplicadas.Sum(p => (double)p.precioDescontado);
                        SetDescuento(descuentoTotal);
                        _logger.LogInformation($"Descuento total aplicado: {descuentoTotal}");
                    }
                    else
                    {
                        SetPromosSession(new List<PromocionDTO>());
                        SetDescuento(0);
                        _logger.LogInformation("No hay promociones aplicables");
                    }
                }
                else
                {
                    SetPromosSession(new List<PromocionDTO>());
                    SetDescuento(0);
                }

                ViewBag.Promociones = GetPromosSession();

                // Calcular subtotal considerando descuentos
                var subtotal = productosSession.Sum(p => p.precio * p.cantidad) - GetDescuento();
                SetSubTotal(subtotal);
                ViewBag.Total = GetSubTotal();

                // Búsqueda de productos si se proporciona nombre
                if (!string.IsNullOrEmpty(nombre))
                {
                    var springApiUrl = _httpClientFactory.CreateClient("SpringApi");
                    var token = HttpContext.Session.GetString("JWToken");
                    springApiUrl.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    
                    var response = await springApiUrl.GetAsync($"/producto/buscarNombre/{nombre.Trim()}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        ViewBag.prodFiltrados =
                            JsonConvert.DeserializeObject<IEnumerable<Producto>>(content);
                    }
                    else
                    {
                        _logger.LogWarning($"Error búsqueda de productos: {response.StatusCode}");
                        ViewBag.Error = "No se pudieron obtener los productos.";
                    }
                }

                return View(new VentaDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en Index: {ex.Message}");
                ViewBag.Error = "Ocurrió un error al cargar la página.";
                return View(new VentaDTO());
            }
        }

        [HttpPost]
        public IActionResult Agregar(ProductoVM productoDTO)
        {
            try
            {
                if (productoDTO == null || productoDTO.cantidad <= 0)
                {
                    ModelState.AddModelError("", "Cantidad inválida");
                    return RedirectToAction("Index");
                }

                _logger.LogInformation($"Producto agregado: {productoDTO.nombre} (Cantidad: {productoDTO.cantidad})");
                SetProductosSession(GetProductosSession().Append(productoDTO).ToList());
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al agregar producto: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(VentaDTO ventaDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(ventaDTO.nombreComprador) || 
                    string.IsNullOrEmpty(ventaDTO.dniComprador))
                {
                    ModelState.AddModelError("", "Complete los datos del comprador");
                    return View("Index", ventaDTO);
                }

                ventaDTO.productos = GetProductosSession();
                ventaDTO.totalPagar = GetSubTotal();
                ventaDTO.descuento = GetDescuento();
                
                var json = JsonConvert.SerializeObject(ventaDTO);
                var springUri = _httpClientFactory.CreateClient("SpringApi");
                var token = HttpContext.Session.GetString("JWToken");
                
                springUri.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await springUri.PostAsync("/venta/registrar", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Venta registrada exitosamente");
                    HttpContext.Session.Remove("Productos");
                    HttpContext.Session.Remove("Promos");
                    HttpContext.Session.Remove("SubTotal");
                    HttpContext.Session.Remove("Descuento");
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error al crear venta: {response.StatusCode} - {errorContent}");
                    ModelState.AddModelError("", "Error al registrar la venta. Intente nuevamente.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción en Create: {ex.Message}");
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            return View("Index", ventaDTO);
        }
    }
}
