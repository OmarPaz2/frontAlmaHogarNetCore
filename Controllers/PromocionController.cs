using AlmaHogarFront.DTO;
using AlmaHogarFront.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AlmaHogarFront.Controllers
{
    public class PromocionController : Controller
    {

        public readonly IHttpClientFactory _httpClientFactory;
        public readonly JsonSerializerOptions _jsonSerializerOptions;


        public PromocionController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IActionResult> Index(string nombre)
        {
            if(!string.IsNullOrEmpty(nombre))
            {
                var netApiUrl = _httpClientFactory.CreateClient("DotNetApi");
                var token = HttpContext.Session.GetString("JWToken");
                netApiUrl.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await netApiUrl.GetAsync($"/api/Promocion/promocionesPorProducto/{nombre}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var promociones = JsonConvert.DeserializeObject<IEnumerable<Product_PromotionVM>>(content);
                    return View(promociones);
                }
            }
            return View();
        }
        public async Task<IActionResult> Create(string nombre, string productosSeleccionados)
        {
            if(!string.IsNullOrEmpty(nombre))
            {
                var springApiUrl = _httpClientFactory.CreateClient("SpringApi");
                var token = HttpContext.Session.GetString("JWToken");
                springApiUrl.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await springApiUrl.GetAsync($"/producto/buscarNombre/{nombre.Trim()}");
                if (response.IsSuccessStatusCode)
                {

                    var content = await response.Content.ReadAsStringAsync();
                    var productosFiltrados = JsonConvert.DeserializeObject<IEnumerable<Producto>>(content);
                    ViewBag.Productos = productosFiltrados;

                    foreach (var item in productosFiltrados)
                    {
                        Console.WriteLine(item.nombre);
                    }
                    return View(new PromoDetalleDtoVM());
                }
            }
        
            if(!string.IsNullOrEmpty(productosSeleccionados))
            {
              var listaPorductos = JsonConvert.DeserializeObject<List<int>>(productosSeleccionados);
                ViewBag.ProductosSeleccionados = listaPorductos;
                
            }
            return View(new PromoDetalleDtoVM());
        }

        [HttpPost]
        public async Task<IActionResult> Create(PromoDetalleDtoVM objeto)
        {
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(objeto);
                var netApiUrl = _httpClientFactory.CreateClient("DotNetApi");
                var token = HttpContext.Session.GetString("JWToken");
                netApiUrl.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await netApiUrl.PostAsync("/api/Promocion/crearPromocion", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Create");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al crear");
                }

            }
               return View(objeto);

        }
    }
}
