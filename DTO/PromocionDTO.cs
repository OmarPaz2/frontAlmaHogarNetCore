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
        
        // Información adicional para mejor contexto
        public List<int>? productosAplicables { get; set; }
        public int? cantidadMinima { get; set; }
        public decimal? montoMinimoCompra { get; set; }
        public string? estado 
        { 
            get 
            { 
                var ahora = DateTime.Now;
                if (ahora < fecha_Inicio) return "Próxima";
                if (ahora > fecha_Fin) return "Expirada";
                return "Activa";
            } 
        }
    }
}
