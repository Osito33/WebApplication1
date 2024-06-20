using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using Rotativa.AspNetCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    public class ComprasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComprasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var inventario = _context.Inventarios.Where(p => p.Cantidad < 5).ToList();
            var pedido = TempData.ContainsKey("Pedido")
                ? JsonSerializer.Deserialize<List<Pedido>>(TempData["Pedido"].ToString())
                : new List<Pedido>();

            TempData.Keep("Pedido"); // Keep TempData

            var viewModel = new CompraViewModel
            {
                Inventario = inventario,
                Pedido = pedido
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AgregarAlPedido(CompraViewModel viewModel)
        {
            var inventario = _context.Inventarios.Where(p => p.Cantidad < 5).ToList();
            viewModel.Inventario = inventario;

            var pedido = TempData.ContainsKey("Pedido")
                ? JsonSerializer.Deserialize<List<Pedido>>(TempData["Pedido"].ToString())
                : new List<Pedido>();

            if (viewModel.NuevoPedido != null && viewModel.NuevoPedido.Cantidad > 0)
            {
                pedido.Add(viewModel.NuevoPedido);
                TempData["Pedido"] = JsonSerializer.Serialize(pedido);
            }

            viewModel.Pedido = pedido;

            TempData.Keep("Pedido"); // Keep TempData

            return View("Index", viewModel);
        }

        public IActionResult MostrarReporte()
        {
            var pedido = TempData.ContainsKey("Pedido")
                ? JsonSerializer.Deserialize<List<Pedido>>(TempData["Pedido"].ToString())
                : new List<Pedido>();

            if (!pedido.Any())
            {
                TempData["Error"] = "No hay productos en el pedido.";
                return RedirectToAction("Index");
            }

            // Debugging: Verifica que los datos estén presentes
            foreach (var item in pedido)
            {
                Debug.WriteLine($"Pedido: {item.Descripcion}, {item.Cantidad}");
            }

            var pdf = new ViewAsPdf("PedidoPDF", pedido)
            {
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };

            return pdf;
        }


    }
}
