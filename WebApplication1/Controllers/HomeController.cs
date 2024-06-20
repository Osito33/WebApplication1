using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Models;
using Rotativa.AspNetCore;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{

    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (TempData["MensajeBienvenida"] != null)
            {
                ViewBag.MensajeBienvenida = TempData["MensajeBienvenida"];
            }
            // Asegúrate de que TempData["UserRole"] esté configurado correctamente
            ViewBag.UserRole = TempData["UserRole"];
            ViewBag.UserName = TempData["UserName"];
            return View(await _context.Inventarios.ToArrayAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            var productoEncontrado = _context.Productos.Find(id);
            if (productoEncontrado != null)
            {
                return View(productoEncontrado);
            }
            else
            {
                return NotFound();
            }
        }

        public IActionResult Delete(int id)
        {
            var productoEncontrado = _context.Productos.Find(id);
            if (productoEncontrado != null)
            {
                return View(productoEncontrado);
            }
            else
            {
                return NotFound();
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddProducto(Producto producto)
        {
            if (ModelState.IsValid)
            {
                // Validar si el NDC digitado ya se encuentra en la tabla de ventas
                bool ndcEnVentas = await _context.Ventas
                    .AnyAsync(v => v.NDC == producto.NDC);

                if (ndcEnVentas)
                {
                    // El NDC ya se encuentra en la tabla de ventas, mostrar mensaje de error
                    TempData["ErrorMessage"] = "El NDC ya se encuentra en las ventas. Favor de verificarlo.";
                    return RedirectToAction("Index");
                }

                // Verificar si el NDC ya está registrado en la tabla Productos
                var ndcExistente = await _context.Productos
                    .FirstOrDefaultAsync(p => p.NDC == producto.NDC);

                if (ndcExistente != null)
                {
                    // Si el NDC ya está registrado, agregar un mensaje de error
                    TempData["ErrorMessage"] = $"El NDC: {producto.NDC} ya está registrado.";

                    return RedirectToAction("Index");
                }

                // Buscar en la tabla Inventarios si existe un producto con la misma descripción, contenido y precio
                var inventarioExistente = await _context.Inventarios
                    .FirstOrDefaultAsync(i => i.Descripcion == producto.Descripcion &&
                                              i.Contenido == producto.Contenido &&
                                              i.Precio == producto.Precio);

                if (inventarioExistente != null)
                {
                    // Si existe, incrementar la cantidad
                    inventarioExistente.Cantidad += 1;
                }
                else
                {
                    // Si no existe, agregar un nuevo registro con cantidad 1
                    var nuevoInventario = new Inventario
                    {
                        Descripcion = producto.Descripcion,
                        Contenido = producto.Contenido,
                        Precio = producto.Precio,
                        Cantidad = 1
                    };
                    _context.Inventarios.Add(nuevoInventario);
                }

                // Agregar el producto a la tabla Productos
                _context.Productos.Add(producto);

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Producto agregado exitosamente.";
                return RedirectToAction("Index");
            }

            return View(producto);
        }
        public IActionResult generarReporte()
        {
            var inventario = _context.Inventarios.ToList();
            var pdf = new ViewAsPdf("generarReporte", inventario)
            {
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
            return pdf;
        }

        [HttpPost]
        public async Task<IActionResult> ProductosReporte(string descripcion, string contenido, int precio)
        {
            var inventario = await _context.Inventarios
                .Where(p => p.Descripcion == descripcion && p.Contenido == contenido && p.Precio == precio)
                .ToListAsync();

            if (!inventario.Any())
            {
                return NotFound();
            }

            var pdf = new ViewAsPdf("ReporteProductos", inventario)
            {
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };

            return pdf;
        }



        [HttpGet]
        public async Task<IActionResult> MostrarProductos(string descripcion, string contenido, int precio)
        {
            var productos = await _context.Productos
                .Where(p => p.Descripcion == descripcion && p.Contenido == contenido && p.Precio == precio)
                .ToListAsync();

            if (productos == null || !productos.Any())
            {
                return NotFound();
            }

            return PartialView("_ProductosModal", productos);
        }

        [HttpPost]
        public async Task<IActionResult> guardarCambio(Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Productos.Update(producto);
                await _context.SaveChangesAsync();
                TempData["ProductEdited"] = "Producto editado exitosamente.";
                return RedirectToAction("Index");
            }
            return View(producto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminarproducto(Producto producto)
        {
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                TempData["ProductDeleted"] = "Producto eliminado exitosamente.";
                return RedirectToAction("Index");
            }
            return View();
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> generateReporte()
        {
            return new ViewAsPdf("GenerateReport", await _context.Productos.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

