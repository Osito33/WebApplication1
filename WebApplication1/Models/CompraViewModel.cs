using System.Collections.Generic;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class CompraViewModel
    {
        public List<Inventario> Inventario { get; set; } = new List<Inventario>();
        public List<Pedido> Pedido { get; set; } = new List<Pedido>();
        public Pedido NuevoPedido { get; set; } = new Pedido();
    }
}
