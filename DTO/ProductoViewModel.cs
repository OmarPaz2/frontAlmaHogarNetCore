namespace AlmaHogarFront.DTO
{
    public class ProductoViewModel
    {
        public int codigo { get; set; }
        public string? nombre { get; set; }
        public string? descripcion { get; set; }
        public double precio { get; set; }
        public int stock { get; set; }
        public int idCategoria { get; set; }
    }
}
