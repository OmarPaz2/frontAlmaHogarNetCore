namespace AlmaHogarFront.Models
{
    public class PromocionVM
    {
        public int id_Promocion { get; set; }
        public string? titulo { get; set; }
        public string? descripcion { get; set; }
        public double descuento { get; set; }
        public DateTime fecha_Inicio { get; set; }
        public DateTime fecha_Fin { get; set; }
        public bool estado /*activo*/ { get; set; }
    }
}
