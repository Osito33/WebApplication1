using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class VentasController : Controller
    {

        private static int contadorVentas = 1;

        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet(Name = "IndexByNumeroVenta")]
        // GET: VentasController
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ventas.ToArrayAsync());
        }


        // GET: VentasController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: VentasController/Create
        public ActionResult Create()
        {
            ViewBag.NoVenta = contadorVentas;
            return View();
        }

        public async Task<IActionResult> AddVenta(Venta venta)
        {
            if (ModelState.IsValid)
            {
                // Validar si el NDC existe en la tabla de productos
                var productoExistente = await _context.Productos.FirstOrDefaultAsync(p => p.NDC == venta.NDC);
                if (productoExistente == null)
                {
                    TempData["ErrorMessage"] = "El NDC ingresado no fue encontrado.";
                    return RedirectToAction("Create");
                }

                // Validar si existe algún producto con la misma descripción y fecha de caducidad más próxima
                var productoRepetido = await _context.Productos.FirstOrDefaultAsync(p => p.Descripcion == venta.Descripcion && p.Caducidad < venta.Caducidad);
                if (productoRepetido != null)
                {
                    TempData["ErrorMessage"] = "Existe un producto con la misma descripción y una fecha de caducidad más próxima. Favor de cambiarlo";
                    return RedirectToAction("Create");
                }

                venta.Total = productoExistente.Precio;

                // Si todas las validaciones pasan, agregar la venta y guardar cambios en la base de datos
                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venta agregada exitosamente.";
                return RedirectToAction("Index", "Ventas");
            }

            // Si la validación falla, configura el mensaje de error en TempData y devuelve la vista Create
            TempData["ErrorMessage"] = "Ha ocurrido un error al agregar la venta. Por favor, revisa los datos e inténtalo de nuevo.";
            return View("Create", venta);
        }



        public async Task<IActionResult> realizarVenta()
        {
            contadorVentas++;

            // Llama a GenerarTicketPDF como una acción del controlador
            var pdfResult = await GenerarTicketPDF(contadorVentas);

            // Espera a que se complete la generación del PDF antes de devolverlo
            return pdfResult;
        }

        public async Task<IActionResult> GenerarTicketPDF(int numeroVenta)
        {
            var productos = await _context.Productos.ToListAsync();

            // Puedes utilizar 'numeroVenta' aquí si es necesario

            // Devuelve el PDF como un archivo para descargar
            return new ViewAsPdf("GenerateTicket", productos);
        }


        // POST: VentasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: VentasController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: VentasController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: VentasController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: VentasController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
