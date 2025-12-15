using AlmaHogarFront.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AlmaHogarFront.Controllers
{
    public class ReporteController : Controller
    {
        public readonly HttpClient _httpClient;
        public readonly JsonSerializerOptions _jsonSerializerOptions;

        public ReporteController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5011");


            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }


        public async Task<IActionResult> Index(string eleccion)
        {
            if (!string.IsNullOrEmpty(eleccion))
            {

                ReporteIndexVM reporteIndexVM = new ReporteIndexVM();

                reporteIndexVM.eleccion = eleccion;

                switch (eleccion)
                {
                    case "masVendidos":
                        reporteIndexVM.productosVendidosMes = await ProductosMasVendidos();
                        break;
                    case "menosVendidos":
                        reporteIndexVM.productosVendidosMes = await ProductosMenosVendidos();
                        break;
                    case "ventasDia":
                        reporteIndexVM.saleByDay = await VentasPorDia();
                        break;
                    default:
                        break;
                };
                return View(reporteIndexVM);

            }
            else
            {
                return View();
            }
        }


        public async Task<IEnumerable<ProductosVendidosMes>> ProductosMasVendidos()
        {
            var response = await _httpClient.GetAsync("api/Reporte/obtenerProductosMasVendidos");

            if (response.IsSuccessStatusCode)
            {

                var content = await response.Content.ReadAsStringAsync();
                var productosMasVendidos = JsonConvert.DeserializeObject<IEnumerable<ProductosVendidosMes>>(content);

                return productosMasVendidos;
            }

            return new List<ProductosVendidosMes>();
        }
        public async Task<IEnumerable<ProductosVendidosMes>> ProductosMenosVendidos()
        {
            var response = await _httpClient.GetAsync("api/Reporte/obtenerProductosMenosVendidos");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var productosMenosVendidos = JsonConvert.DeserializeObject<IEnumerable<ProductosVendidosMes>>(content);
                return productosMenosVendidos;
            }
            return new List<ProductosVendidosMes>();
        }

        public async Task<IEnumerable<SalesByDay>> VentasPorDia()
        {
            var response = await _httpClient.GetAsync("api/Reporte/obtenerVentasDia");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var ventasPorDia = JsonConvert.DeserializeObject<IEnumerable<SalesByDay>>(content);
                return ventasPorDia;
            }
            return new List<SalesByDay>();
        }

     
    }
}
