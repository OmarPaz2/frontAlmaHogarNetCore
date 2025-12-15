namespace AlmaHogarFront.Models
{
    public class ReporteIndexVM
    {
        public IEnumerable<ProductosVendidosMes> productosVendidosMes{ get; set; }
        public IEnumerable<SalesByDay> saleByDay{ get; set; }

        public string? eleccion { get; set; }
    }
}
