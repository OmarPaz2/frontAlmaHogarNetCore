using AlmaHogarFront.DTO;
using AlmaHogarFront.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NuGet.Protocol;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AlmaHogarFront.Controllers
{
    public class VentaController : Controller
    {
        public readonly IHttpClientFactory _httpClientFactory;
        public readonly JsonSerializerOptions _jsonSerializerOptions;
       
        public VentaController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

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


        public async Task<IEnumerable<PromocionDTO>> getPromocionByIdProduct(List<ProductoCompraDTO> productos)
        {
           
            var json = JsonConvert.SerializeObject(productos);
            var netApiUrl = _httpClientFactory.CreateClient("DotNetApi");
            var token = HttpContext.Session.GetString("JWToken");
            netApiUrl.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await netApiUrl.PostAsync("/api/Promocion/obtenerPromocionesAplicadas", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var promociones = JsonConvert.DeserializeObject<IEnumerable<PromocionDTO>>(responseContent);
                return promociones;
            }
            else
            {
                return Enumerable.Empty<PromocionDTO>();
            }
        }


        public async Task<IActionResult> Index(string nombre)
        {
            var productosSession = GetProductosSession();

            Console.WriteLine("Cantidad de datos en la lista : " + productosSession.Count);
 
            ViewBag.ListaProd = productosSession;
    
            if (productosSession.Any())
            {
                var prodCompraList = productosSession.Select(item => new ProductoCompraDTO
                {
                    idProducto = item.codigo,
                    cantidad = item.cantidad
                }).ToList();

                var promocionesAplicadas = await getPromocionByIdProduct(prodCompraList);

                if (promocionesAplicadas != null)
                {
                    SetPromosSession(promocionesAplicadas.ToList());
                    SetDescuento(promocionesAplicadas.Sum(p => (double)p.precioDescontado));
                }
            }
            else
            {
                SetPromosSession(new List<PromocionDTO>());
                SetDescuento(0);
            }

            ViewBag.Promociones = GetPromosSession();

            SetSubTotal(
                productosSession.Sum(p => p.precio * p.cantidad) - GetDescuento()
            );
            ViewBag.Total = GetSubTotal();

            if (!string.IsNullOrEmpty(nombre))
            {
               
                var springApiUrl = _httpClientFactory.CreateClient("SpringApi");
                var token = HttpContext.Session.GetString("JWToken");
                springApiUrl.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await springApiUrl.GetAsync($"/producto/buscarNombre/{nombre.Trim()}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ViewBag.prodFiltrados =
                        JsonConvert.DeserializeObject<IEnumerable<Producto>>(content);
                }
                else
                {
                    ViewBag.Error = "No se pudieron obtener los productos.";
                }
            }

            return View(new VentaDTO());

        }
        [HttpPost]
        public IActionResult Agregar(ProductoVM productoDTO)
        {
            Console.WriteLine("Producto agregado: " + productoDTO.nombre);
            SetProductosSession(GetProductosSession().Append(productoDTO).ToList());
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Create(VentaDTO ventaDTO)
        {
            ventaDTO.productos = GetProductosSession();
            ventaDTO.totalPagar = GetSubTotal();
            ventaDTO.descuento = GetDescuento();
            
                var json = JsonConvert.SerializeObject(ventaDTO);
                var springUri = _httpClientFactory.CreateClient("SpringApi");
            var token = HttpContext.Session.GetString("JWToken");
            springUri.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await springUri.PostAsync("/venta/registrar", content);

                if (response.IsSuccessStatusCode)
                {
                    HttpContext.Session.Remove("Productos");
                    HttpContext.Session.Remove("Promos");
                    return View("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Error al crear");
                }

            
            return View("Index",ventaDTO);

        }
    }
}
