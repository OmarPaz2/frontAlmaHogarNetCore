using AlmaHogarFront.DTO;
using AlmaHogarFront.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AlmaHogarFront.Controllers
{
    public class ProductoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductoController> _logger;

        public ProductoController(
            IHttpClientFactory httpClientFactory,
            ILogger<ProductoController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el token JWT de la sesión
        /// </summary>
        private string GetTokenFromSession()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token JWT no disponible en sesión");
            }
            return token ?? string.Empty;
        }

        /// <summary>
        /// Obtiene un cliente HttpClient configurado con autenticación
        /// </summary>
        private HttpClient GetAuthorizedClient(string clientName)
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var token = GetTokenFromSession();

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de productos");

                var client = GetAuthorizedClient("SpringApi");
                var response = await client.GetAsync("/producto/allProductos");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var productos = JsonConvert.DeserializeObject<List<Producto>>(content);

                    _logger.LogInformation($"Se obtuvieron {productos?.Count ?? 0} productos");
                    return View("Index", productos ?? new List<Producto>());
                }
                else
                {
                    _logger.LogError($"Error obteniendo productos: {response.StatusCode}");
                    ViewBag.Error = "No se pudieron obtener los productos";
                    return View(new List<Producto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción en Index: {ex.Message}");
                ViewBag.Error = "Ocurrió un error al cargar los productos";
                return View(new List<Producto>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Cargando formulario de creación de producto");

                var client = GetAuthorizedClient("SpringApi");
                var response = await client.GetAsync("/producto/allProductos");

                var categorias = new List<Categoria>();

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var productos = JsonConvert.DeserializeObject<List<Producto>>(content);

                    // Extraer categorías únicas
                    categorias = productos?
                        .Where(p => p.categoria != null)
                        .Select(p => p.categoria)
                        .GroupBy(c => c.codigo)
                        .Select(g => g.First())
                        .ToList() ?? new List<Categoria>();

                    _logger.LogInformation($"Se obtuvieron {categorias.Count} categorías");
                }
                else
                {
                    _logger.LogWarning($"Error obteniendo categorías: {response.StatusCode}");
                }

                ViewBag.Categorias = categorias;
                return View(new ProductoViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción en Create GET: {ex.Message}");
                ViewBag.Error = "Error al cargar el formulario";
                return View(new ProductoViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductoViewModel productoViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido al crear producto");

                    // Recargar categorías para la vista
                    var client = GetAuthorizedClient("SpringApi");
                    var response = await client.GetAsync("/producto/allProductos");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var productos = JsonConvert.DeserializeObject<List<Producto>>(content);
                        var categorias = productos?
                            .Where(p => p.categoria != null)
                            .Select(p => p.categoria)
                            .GroupBy(c => c.codigo)
                            .Select(g => g.First())
                            .ToList() ?? new List<Categoria>();
                        ViewBag.Categorias = categorias;
                    }

                    return View(productoViewModel);
                }

                _logger.LogInformation($"Creando producto: {productoViewModel.nombre}");

                var httpClient = GetAuthorizedClient("SpringApi");
                var json = JsonConvert.SerializeObject(productoViewModel);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                var createResponse = await httpClient.PostAsync("/producto/registrarProducto", stringContent);

                if (createResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Producto creado exitosamente: {productoViewModel.nombre}");
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await createResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Error creando producto: {createResponse.StatusCode} - {errorContent}");
                    ModelState.AddModelError(string.Empty, "Error al crear el producto. Intente nuevamente.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción en Create POST: {ex.Message}");
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            }

            // Recargar categorías para la vista
            try
            {
                var client = GetAuthorizedClient("SpringApi");
                var response = await client.GetAsync("/producto/allProductos");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var productos = JsonConvert.DeserializeObject<List<Producto>>(content);
                    var categorias = productos?
                        .Where(p => p.categoria != null)
                        .Select(p => p.categoria)
                        .GroupBy(c => c.codigo)
                        .Select(g => g.First())
                        .ToList() ?? new List<Categoria>();
                    ViewBag.Categorias = categorias;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error recargando categorías: {ex.Message}");
            }

            return View(productoViewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation($"Cargando producto para editar: {id}");

                var client = GetAuthorizedClient("SpringApi");

                // 🔹 1. Obtener el producto por ID
                var response = await client.GetAsync($"/producto/buscarIdProduct/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Producto no encontrado: {id}");
                    return RedirectToAction("Index");
                }

                var content = await response.Content.ReadAsStringAsync();
                var producto = JsonConvert.DeserializeObject<ProductoViewModel>(content);

                if (producto == null)
                {
                    _logger.LogWarning($"Error deserializando producto: {id}");
                    return NotFound();
                }

                // 🔹 2. Obtener TODOS los productos (para sacar categorías)
                var responseProductos = await client.GetAsync("/producto/allProductos");

                var categorias = new List<Categoria>();

                if (responseProductos.IsSuccessStatusCode)
                {
                    var contentProductos = await responseProductos.Content.ReadAsStringAsync();
                    var productos = JsonConvert.DeserializeObject<List<ProductoViewModel>>(contentProductos);

                    // 🔹 3. Extraer categorías únicas
                    // CAMBIO: No existe 'categoria' en ProductoViewModel, así que no se puede acceder así.
                    // Debes obtener las categorías de otra manera, por ejemplo, desde un endpoint específico o usando los productos originales si tienen la propiedad.
                    // Aquí se asume que necesitas obtener las categorías de los productos deserializados como 'Producto' (no 'ProductoViewModel').

                    // Ejemplo de corrección:
                    var productosOriginales = JsonConvert.DeserializeObject<List<Producto>>(contentProductos);
                    categorias = productosOriginales?
                        .Where(p => p.categoria != null)
                        .Select(p => p.categoria)
                        .GroupBy(c => c.codigo)
                        .Select(g => g.First())
                        .ToList() ?? new List<Categoria>();
                }

                // 🔹 4. Enviar categorías a la vista
                ViewBag.Categorias = categorias;

                _logger.LogInformation($"Producto cargado para editar: {producto.nombre}");
                return View(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción en Edit GET: {ex.Message}");
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductoViewModel productoViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"ModelState inválido al editar producto {id}");
                    return View(productoViewModel);
                }

                _logger.LogInformation($"Actualizando producto: {id} - {productoViewModel.nombre}");

                var client = GetAuthorizedClient("SpringApi");
                var json = JsonConvert.SerializeObject(productoViewModel);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"/producto/editProducto/{id}", stringContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Producto actualizado exitosamente: {id}");
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error actualizando producto: {response.StatusCode} - {errorContent}");
                    ModelState.AddModelError(string.Empty, "Error al actualizar el producto. Intente nuevamente.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción en Edit POST: {ex.Message}");
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            }

            return View(productoViewModel);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando producto: {id}");

                var client = GetAuthorizedClient("SpringApi");
                var response = await client.DeleteAsync($"/producto/eliminar/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Producto eliminado exitosamente: {id}");
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogError($"Error eliminando producto: {response.StatusCode}");
                    ModelState.AddModelError(string.Empty, "Error al eliminar el producto. Intente nuevamente.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción en Delete: {ex.Message}");
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            }

            return RedirectToAction("Index");
        }
    }
}
