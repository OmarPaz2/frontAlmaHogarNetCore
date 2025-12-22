namespace AlmaHogarFront.DTO
{
    public class PromoDetalleDtoVM
    {
        public string? titulo { get; set; }
        public string? descripcion { get; set; }
        public double descuento { get; set; }
        public DateOnly? fecha_Inicio { get; set; }
        public DateOnly?fecha_Fin { get; set; }
        public bool estado /*activo*/ { get; set; }

        public List<Product_PromotionDtoVM> Product_PromotionDTOs { get; set; }
    }
}
