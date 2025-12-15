using System.ComponentModel;

namespace AlmaHogarFront.Models
{
    public class ProductosVendidosMes
    {
        [DisplayName("Id")] public int id_producto { get; set; }
        [DisplayName("Nombre")] public string? nom_producto { get; set; }
        [DisplayName("Vendidos")] public int total_vendido { get; set; }
    }
}
