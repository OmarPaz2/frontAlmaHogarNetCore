using AlmaHogarFront.DTO;
using Newtonsoft.Json;
using System.Text;

namespace AlmaHogarFront.Services
{
    public interface IPromocionService
    {
        Task<IEnumerable<PromocionDTO>> ObtenerPromocionesAplicadas(List<ProductoCompraDTO> productos, string token);
    }

    public class PromocionService : IPromocionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PromocionService> _logger;

        public PromocionService(IHttpClientFactory httpClientFactory, ILogger<PromocionService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene las promociones aplicables para los productos del carrito
        /// El backend ya filtra automáticamente las que no cumplen condiciones
        /// </summary>
        public async Task<IEnumerable<PromocionDTO>> ObtenerPromocionesAplicadas(
            List<ProductoCompraDTO> productos, 
            string token)
        {
            try
            {
                if (productos == null || productos.Count == 0)
                {
                    _logger.LogInformation("No hay productos para obtener promociones");
                    return Enumerable.Empty<PromocionDTO>();
                }

                var json = JsonConvert.SerializeObject(productos);
                var client = _httpClientFactory.CreateClient("DotNetApi");
                
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("/api/Promocion/obtenerPromocionesAplicadas", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var promociones = JsonConvert.DeserializeObject<IEnumerable<PromocionDTO>>(responseContent);
                    
                    _logger.LogInformation($"Se obtuvieron {promociones?.Count()} promociones aplicadas");
                    return promociones ?? Enumerable.Empty<PromocionDTO>();
                }
                else
                {
                    _logger.LogWarning($"Error al obtener promociones. Status: {response.StatusCode}");
                    return Enumerable.Empty<PromocionDTO>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción al obtener promociones: {ex.Message}");
                return Enumerable.Empty<PromocionDTO>();
            }
        }
    }
}
