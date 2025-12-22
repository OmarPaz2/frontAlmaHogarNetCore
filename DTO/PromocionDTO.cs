namespace AlmaHogarFront.DTO
{
    public class PromocionDTO
    {
        public int id_Promocion { get; set; }
        public string? titulo { get; set; }
        public string? descripcion { get; set; }
        public decimal descuento { get; set; }
        public decimal precioDescontado { get; set; }
        public DateTime fecha_Inicio { get; set; }
        public DateTime fecha_Fin { get; set; }
    }
}
