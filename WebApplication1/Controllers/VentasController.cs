using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System.IO;
using Rotativa.AspNetCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;


namespace WebApplication1.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {

        // Crear una lista para almacenar NDCs
        private List<string> ndcList = new List<string>();
        private static int contadorVentas = 0;

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        private static int totalVentas = 0;

        public VentasController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        [Authorize]
        public IActionResult Index()
        {
            // Obtener el valor máximo de NoVenta de la base de datos
            contadorVentas = _context.Contadors.Max(v => (int?)v.contadorVentas) ?? 0;

            // Obtener todas las ventas de tu base de datos que coincidan con el contadorVentas
            var ventas = _context.Ventas.Where(v => v.NoVenta == contadorVentas).ToList();

            // Calcular el total de ventas sumando los totales de todas las ventas
            decimal totalVentas = ventas.Sum(v => v.Total);

            // Crear una instancia de VentasViewModel y asignar las ventas y el total de ventas
            var ventasViewModel = new VentasViewModel
            {
                Ventas = ventas,
                TotalVentas = totalVentas
            };

            // Pasar el ViewModel a la vista
            return View(ventasViewModel);
        }

        // GET: VentasController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: VentasController/Create
        public ActionResult Create(Producto producto)
        {
            ViewBag.NoVenta = contadorVentas;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Buscar(Producto producto)
        {

            // Buscar el producto en la base de datos por NDC
            var productoEncontrado = await _context.Productos
                .FirstOrDefaultAsync(p => p.NDC == producto.NDC);

            if (productoEncontrado != null)
            {
                // Validar si existe algún producto con la misma descripción y fecha de caducidad más próxima
                var productoRepetido = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Descripcion == productoEncontrado.Descripcion && p.Caducidad < productoEncontrado.Caducidad);

                if (productoRepetido != null)
                {
                    // Verificar si el producto repetido no se encuentra ya en la tabla de ventas con el NDC
                    bool productoEnVentas = await _context.Ventas
                        .AnyAsync(v => v.NDC == productoRepetido.NDC);

                    if (!productoEnVentas)
                    {
                        // El producto repetido no se encuentra en la tabla de ventas
                        TempData["ErrorMessage"] = "Existe un producto con la misma descripción y una fecha de caducidad más próxima. Favor de cambiarlo.";
                        return RedirectToAction("Index");
                    }
                }


                // Crear la instancia de Venta con los datos del producto encontrado y redirigir a la acción Create
                var venta = new Venta
                {
                    NoVenta = contadorVentas, // Asegúrate de asignar correctamente el NoVenta aquí
                    NDC = productoEncontrado.NDC,
                    Descripcion = productoEncontrado.Descripcion,
                    Contenido = productoEncontrado.Contenido,
                    Caducidad = productoEncontrado.Caducidad,
                    Precio = productoEncontrado.Precio
                };

                ViewBag.NoVenta = contadorVentas; // Asegúrate de que contadorVentas esté asignado correctamente
                return View("Create", venta);

            }
            else
            {
                // El producto no existe, mostrar mensaje de error
                TempData["ErrorMessage"] = "El NDC no fue encontrado.";
                return RedirectToAction("Index"); // O redirigir a la vista que desees
            }
        }


        public async Task<IActionResult> AddVenta(Venta venta)
        {
            if (ModelState.IsValid)
            {
                //verifica si el producto existe en la tabla de productos por medio del NDC
                var productoExistente = await _context.Productos.FirstOrDefaultAsync(p => p.NDC == venta.NDC);
                if (productoExistente != null)
                {
                    // Agregar la fecha actual a la venta
                    venta.FechaVenta = DateTime.Now;
                    //agrega el precio al total
                    venta.Total = productoExistente.Precio;
                    // Actualiza el total de ventas
                    totalVentas = totalVentas + productoExistente.Precio;
                    // Agregar el NDC a la lista de NDCs
                    ndcList.Add(venta.NDC);
                }

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                // Convertir la lista de NDCs a una cadena separada por comas
                string ndcListString = string.Join(", ", ndcList);
                TempData["SuccessMessage"] = $"Venta agregada exitosamente. NDCs: {ndcListString}";
                //TempData["SuccessMessage"] = "Venta agregada exitosamente.";

                return RedirectToAction("Index", "Ventas");
            }

            TempData["ErrorMessage"] = "Ha ocurrido un error al agregar la venta. Por favor, revisa los datos e inténtalo de nuevo.";
            return View("Create", venta);
        }


        public async Task<IActionResult> ObtenerTotalVenta(int noVenta)
        {
            var totalVenta = await _context.Ventas
                .Where(v => v.NoVenta == noVenta)
                .SumAsync(v => v.Total);

            return Json(totalVenta);
        }

        public async Task<IActionResult> realizarVenta()
        {
            // Obtener la fecha y hora actual
            DateTime fechaHoraActual = DateTime.Now;
            // Obtener todas las ventas de tu base de datos que coincidan con el contadorVentas
            var ventas = _context.Ventas.Where(v => v.NoVenta == contadorVentas).ToList();

            // Validar si hay ventas registradas
            if (!ventas.Any())
            {
                TempData["ErrorMessage"] = "No hay ventas registradas para procesar.";
                return RedirectToAction("Index", "Ventas");
            }

            // Iterar sobre cada venta
            foreach (var venta in ventas)
            {
                // Obtener los productos de la venta
                var productosVenta = await _context.Productos.Where(p => p.NDC == venta.NDC).ToListAsync();

                // Iterar sobre cada producto de la venta
                foreach (var productoVenta in productosVenta)
                {
                    // Buscar el producto en la tabla de inventarios
                    var productoInventario = await _context.Inventarios.FirstOrDefaultAsync(i =>
                         i.Descripcion == productoVenta.Descripcion && i.Precio == productoVenta.Precio);

                    // Si el producto se encuentra en el inventario
                    if (productoInventario != null)
                    {
                        // Restar uno a la cantidad en inventario
                        productoInventario.Cantidad--;
                    }
                }
            }
            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            // Eliminar los productos que tengan los NDC de la lista ndcList
            foreach (var venta in ventas)
            {
                var productosAEliminar = await _context.Productos.Where(p => p.NDC == venta.NDC).ToListAsync();
                if (productosAEliminar.Any())
                {
                    _context.Productos.RemoveRange(productosAEliminar);
                }
                
            }
            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            // Reiniciar el total de ventas
            totalVentas = 0;
            //Aumenta el contador 
            contadorVentas++;
            // Crear un nuevo registro en la tabla Contadors con el valor actual del contador de ventas
            var nuevoContador = new Contador { contadorVentas = contadorVentas };
            _context.Contadors.Add(nuevoContador);
            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            // Llama a GenerarTicketPDF como una acción del controlador
            var pdfBytes = await GenerarTicketPDF(contadorVentas);
            // Nombre del archivo del ticket con el contador de ventas y la fecha y hora actuales
            string nombreArchivo = $"Ticket_{contadorVentas}_{fechaHoraActual.ToString("yyyy-MM-dd_HH-mm-ss")}.pdf";
            // Ruta completa del directorio Tickets dentro de wwwroot
            string ticketsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Tickets");

            // Si el directorio no existe, créalo
            if (!Directory.Exists(ticketsFolder))
            {
                Directory.CreateDirectory(ticketsFolder);
            }

            // Ruta completa del archivo PDF dentro del directorio Tickets
            var filePath = Path.Combine(ticketsFolder, nombreArchivo);
            // Guarda el archivo PDF en la carpeta Tickets dentro de wwwroot
            System.IO.File.WriteAllBytes(filePath, pdfBytes);


            // Aquí puedes redirigir a una página de confirmación o similar
            TempData["SuccessMessage"] = $"Venta realizada exitosamente. Ticket guardado como {nombreArchivo}";
            return RedirectToAction("Index", "Ventas");
        }

        public async Task<IActionResult> cancelarVenta()
        {
            // Obtener todas las ventas de tu base de datos que coincidan con el contadorVentas
            var ventas = _context.Ventas.Where(v => v.NoVenta == contadorVentas).ToList();

            // Validar si hay ventas registradas
            if (!ventas.Any())
            {
                TempData["ErrorMessage"] = "No hay ventas registradas para procesar.";
                return RedirectToAction("Index", "Ventas");
            }

            // Eliminar las ventas que coincidan con el contadorVentas
            _context.Ventas.RemoveRange(ventas);
            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            // Reiniciar el total de ventas
            totalVentas = 0;
            return RedirectToAction("Index", "Ventas");

        }

        public async Task<byte[]> GenerarTicketPDF(int numeroVenta)
        {
            // Obtener la fecha actual sin la parte de tiempo
            DateTime fechaActual = DateTime.Today;

            // Filtrar las ventas por el número de venta y la fecha de hoy
            var ventas = await _context.Ventas
                .Where(v => v.NoVenta == (numeroVenta-1) && v.FechaVenta.Date == fechaActual)
                .ToListAsync();

            // Genera el PDF y conviértelo en un byte[]
            var pdfBytes = await new ViewAsPdf("GenerateTicket", ventas).BuildFile(ControllerContext);

            return pdfBytes;
        }

    }

}



