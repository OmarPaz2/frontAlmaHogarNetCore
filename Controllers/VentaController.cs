using AlmaHogarFront.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;

namespace AlmaHogarFront.Controllers
{
    public class VentaController : Controller
    {
        public readonly HttpClient _httpClient;
        public readonly JsonSerializerOptions _jsonSerializerOptions;
        private static List<ProductoDTO> prodSeleccionados= new();

        public VentaController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:8080");


            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }
        public async Task<IActionResult> Index(string nombre)
        {
            Console.WriteLine("Cantidad de datos en la lista : " + prodSeleccionados.Count);

            ViewBag.ListaProd = prodSeleccionados;
            ViewBag.Total = prodSeleccionados.Sum(p => p.precio * p.cantidad);
            if (string.IsNullOrEmpty(nombre))
            {
                return View(Enumerable.Empty<Producto>());
            }
            else
            {
                var response = await _httpClient.GetAsync($"/producto/buscarNombre/{nombre.Trim()}");

                if (response.IsSuccessStatusCode)
                {

                    var content = await response.Content.ReadAsStringAsync();
                    var productosFiltrados = JsonConvert.DeserializeObject<IEnumerable<Producto>>(content);
                    ViewBag.ListaProd = prodSeleccionados;
                    return View(productosFiltrados);
                }
                ViewBag.Error = "No se pudieron obtener los productos.";
                return View();
            }
            
        }
        [HttpPost]
        public IActionResult Agregar(ProductoDTO productoDTO)
        {
            Console.WriteLine("Producto agregado: " + productoDTO.nombre);
            prodSeleccionados.Add(productoDTO);
            return RedirectToAction("Index");
        }
    }
}
