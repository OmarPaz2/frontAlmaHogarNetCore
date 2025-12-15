using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AlmaHogarFront.Models
{
    public class SalesByDay
    {
        [DisplayName("ID")] public int id_venta { get; set; }
        [DisplayName("Fecha")] public DateTime fecha_venta { get; set; }
        [DisplayName("DNI")] public string? dnicomprador { get; set; }
        [DisplayName("Comprador")] public string? comprador { get; set; }
        [DisplayName("Celular")] public string? celular { get; set; }
        [DisplayName("Correo")] public string? correo { get; set; }
        [DisplayName("ID Vendedor")] public int id_user { get; set; }
        [DisplayName("Producto")] public string? nom_producto { get; set; }
        [DisplayName("Cantidad")] public int cantidad_detalle { get; set; }
        [DisplayName("subTotal")] public decimal sub_total_detalle { get; set; }
    }
}
