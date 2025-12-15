namespace AlmaHogarFront.Models
{
    public class Producto
    {


        public int codigo { get; set; }
        public string? nombre { get; set; }
        public string? descripcion { get; set; }
        public double precio { get; set; }
        public int stock { get; set; }
        public Categoria? categoria { get; set; }
    }
}
