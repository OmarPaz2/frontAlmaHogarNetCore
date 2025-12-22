using AlmaHogarFront.Models;
using Humanizer;

namespace AlmaHogarFront.DTO
{
    public class VentaDTO
    {
        public int idVendedor { get; set; }
        public string? nombreComprador {  get; set; }    
        public string? dniComprador { get; set; }
        public string? correoComprador { get; set; }
        public string? telefonoComprador { get; set; }
        public IEnumerable<ProductoVM>? productos { get; set; }
        public double totalPagar { get; set; }
        public double descuento { get; set; }
    }
}
