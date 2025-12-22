namespace AlmaHogarFront.Models
{
    public class Product_PromotionVM
    {
        public int id_detalle_promocion { get; set; }
        public int cantidad_minima { get; set; }

        public int Id_Producto { get; set; }
        public string? nomProduct { get; set; }

        public int Id_Promocion { get; set; }

     
    }
}
