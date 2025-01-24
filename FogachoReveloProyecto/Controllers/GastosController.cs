using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FogachoReveloProyecto.Models;

namespace FogachoReveloProyecto.Controllers
{
    public class GastosController : Controller
    {
        private readonly FogachoReveloDataBase _context;

        public GastosController(FogachoReveloDataBase context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: Gastos/PaginaInicial
        public async Task<IActionResult> PaginaInicial(string categoria)
        {
            var userId = TempData["UserId"] as int?;
            var nombreUsuario = TempData["NombreUsuario"] as string;

            if (userId == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var gastosQuery = _context.Gasto
                .Include(g => g.Usuario)
                .Where(g => g.IdUsuario == userId);

            if (!string.IsNullOrEmpty(categoria) && Enum.TryParse<Categoria>(categoria, out var categoriaEnum))
            {
                gastosQuery = gastosQuery.Where(g => g.Categorias == categoriaEnum);
            }

            var subtotalGastos = await gastosQuery.SumAsync(g => g.Valor ?? 0);
            var subtotalValorPagado = await gastosQuery.SumAsync(g => g.ValorPagado);
            var total = subtotalGastos - subtotalValorPagado;

            ViewBag.SubtotalGastos = subtotalGastos;
            ViewBag.SubtotalValorPagado = subtotalValorPagado;
            ViewBag.Total = total;
            ViewBag.NombreUsuario = nombreUsuario;
            ViewBag.UserId = userId; // Pasar el userId a la vista

            return View(await gastosQuery.ToListAsync());
        }

        // GET: Gastos/CrearGasto
        [HttpGet]
        public IActionResult CrearGasto(int userId)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            return View(new Gasto
            {
                FechaRegristo = DateTime.Now,
                IdUsuario = userId
            });
        }

        // POST: Gastos/CrearGasto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearGasto([Bind("FechaRegristo,FechaFinal,Categorias,Descripcion,Valor,ValorPagado,Estados,IdUsuario")] Gasto gasto)
        {
            if (gasto.IdUsuario == 0)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            if (!ModelState.IsValid)
            {
                return View(gasto);
            }

            try
            {
                gasto.ValidarValor();
                _context.Add(gasto);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Gasto creado exitosamente.";
                return RedirectToAction(nameof(PaginaInicial), new { userId = gasto.IdUsuario });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear el gasto: " + ex.Message);
                return View(gasto);
            }
        }

        // GET: Gastos/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int userId)
        {
            if (!id.HasValue || userId == 0)
            {
                return BadRequest("ID de gasto o usuario no proporcionado.");
            }

            var gasto = await _context.Gasto
                .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

            if (gasto == null)
            {
                return NotFound($"No se encontró el gasto con ID: {id}");
            }

            return View(gasto);
        }

        // POST: Gastos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdGasto,FechaRegristo,FechaFinal,Categorias,Descripcion,Valor,ValorPagado,Estados,IdUsuario")] Gasto gasto)
        {
            if (id != gasto.IdGasto || gasto.IdUsuario == 0)
            {
                return BadRequest("ID de gasto o usuario no coincide.");
            }

            if (!ModelState.IsValid)
            {
                return View(gasto);
            }

            try
            {
                gasto.ValidarValor();
                _context.Update(gasto);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Gasto actualizado exitosamente.";
                TempData["UserId"] = gasto.IdUsuario; // Guardar el userId en TempData
                TempData.Keep("UserId"); // Mantener el userId para la próxima solicitud

                return RedirectToAction(nameof(PaginaInicial), new { userId = gasto.IdUsuario });
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Error de concurrencia al actualizar el gasto.");
                return View(gasto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar el gasto: " + ex.Message);
                return View(gasto);
            }
        }

        // GET: Gastos/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id, int userId)
        {
            if (!id.HasValue || userId == 0)
            {
                return BadRequest("ID de gasto o usuario no proporcionado.");
            }

            var gasto = await _context.Gasto
                .Include(g => g.Usuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdGasto == id && m.IdUsuario == userId);

            if (gasto == null)
            {
                return NotFound($"No se encontró el gasto con ID: {id}");
            }

            return View(gasto);
        }

        // POST: Gastos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int userId)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var gasto = await _context.Gasto
                .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

            if (gasto == null)
            {
                return NotFound($"No se encontró el gasto con ID: {id}");
            }

            try
            {
                _context.Gasto.Remove(gasto);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Gasto eliminado exitosamente.";
                return RedirectToAction(nameof(PaginaInicial), new { userId = userId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el gasto: " + ex.Message;
                return RedirectToAction(nameof(PaginaInicial), new { userId = userId });
            }
        }

        private async Task<bool> GastoExists(int id, int userId)
        {
            return await _context.Gasto.AnyAsync(e => e.IdGasto == id && e.IdUsuario == userId);
        }
    }
}