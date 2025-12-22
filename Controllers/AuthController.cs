using AlmaHogarFront.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace AlmaHogarFront.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5011");
            //_httpClient.BaseAddress = new Uri("https://localhost:7245");
        
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/Api/Usuario/ValidarAcceso", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Credenciales inválidas");
                return View(model);
            }

            var result = JsonConvert.DeserializeObject<dynamic>(
                await response.Content.ReadAsStringAsync()
            );

            // GUARDAR TOKEN
            HttpContext.Session.SetString("JWToken", (string)result.token);

            return RedirectToAction("Index", "Reporte");
        }

    }
}
