using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class VentasViewModel
    {
        public IEnumerable<Venta> Ventas { get; set; }
        public List<Producto> ProductosEnVenta { get; set; }
        public decimal TotalVentas { get; set; }
    }
}
