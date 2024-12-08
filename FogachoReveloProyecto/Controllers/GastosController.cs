using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // GET: Gasto
        public async Task<IActionResult> PaginaInicial(string categoria)
        {
            var gastos = from g in _context.Gasto select g;

            if (!string.IsNullOrEmpty(categoria) && Enum.TryParse<Categoria>(categoria, out var categoriaEnum))
            {
                gastos = gastos.Where(g => g.Categorias == categoriaEnum);
            }

            var subtotalGastos = await gastos.SumAsync(g => g.Valor);
            var subtotalValorPagado = await gastos.SumAsync(g => g.ValorPagado);
            var total = subtotalGastos - subtotalValorPagado;

            ViewBag.SubtotalGastos = subtotalGastos;
            ViewBag.SubtotalValorPagado = subtotalValorPagado;
            ViewBag.Total = total;

            return View(await gastos.ToListAsync());
        }

        [HttpGet]
        public IActionResult CrearGasto()
        {
            return View(new Gasto { FechaRegristo = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearGasto([Bind("FechaRegristo,FechaFinal,Categorias,Descripcion,Valor,ValorPagado,Estados")] Gasto gasto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(gasto);
                }

                gasto.ValidarValor();
                await _context.AddAsync(gasto);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Gasto creado exitosamente.";
                return RedirectToAction(nameof(PaginaInicial));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear el gasto: " + ex.Message);
                return View(gasto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                return BadRequest("ID de gasto no proporcionado.");
            }

            var gasto = await _context.Gasto
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdGasto == id);

            if (gasto == null)
            {
                return NotFound($"No se encontró el gasto con ID: {id}");
            }

            return View(gasto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return BadRequest("ID de gasto no proporcionado.");
            }

            var gasto = await _context.Gasto.FindAsync(id);

            if (gasto == null)
            {
                return NotFound($"No se encontró el gasto con ID: {id}");
            }

            return View(gasto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdGasto,FechaRegristo,FechaFinal,Categorias,Descripcion,Valor,ValorPagado,Estados")] Gasto gasto)
        {
            if (id != gasto.IdGasto)
            {
                return BadRequest("ID de gasto no coincide.");
            }

            if (!await GastoExists(id))
            {
                return NotFound($"No se encontró el gasto con ID: {id}");
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
                return RedirectToAction(nameof(PaginaInicial));
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

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                return BadRequest("ID de gasto no proporcionado.");
            }

            var gasto = await _context.Gasto
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdGasto == id);

            if (gasto == null)
            {
                return NotFound($"No se encontró el gasto con ID: {id}");
            }

            return View(gasto);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gasto = await _context.Gasto.FindAsync(id);

            if (gasto == null)
            {
                return NotFound($"No se encontró el gasto con ID: {id}");
            }

            try
            {
                _context.Gasto.Remove(gasto);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Gasto eliminado exitosamente.";
                return RedirectToAction(nameof(PaginaInicial));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el gasto: " + ex.Message;
                return RedirectToAction(nameof(PaginaInicial));
            }
        }

        private async Task<bool> GastoExists(int id)
        {
            return await _context.Gasto.AnyAsync(e => e.IdGasto == id);
        }
    }
}