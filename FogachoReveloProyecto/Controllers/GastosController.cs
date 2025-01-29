using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FogachoReveloProyecto.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System;

namespace FogachoReveloProyecto.Controllers
{
    public class GastosController : Controller
    {
        private readonly FogachoReveloDataBase _context;
        private readonly IDataProtector _protector;

        public GastosController(FogachoReveloDataBase context, IDataProtectionProvider provider)
        {
            _context = context;
            _protector = provider.CreateProtector("Monetix.Usuario");
        }

        // GET: Gastos/PaginaInicial
        public async Task<IActionResult> PaginaInicial(Categoria? categoria)
        {
            var userId = GetUserIdFromCookies();
            if (userId == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var nombreUsuario = Request.Cookies["NombreUsuario"];

            IQueryable<Gasto> query = _context.Gasto
                .Where(g => g.IdUsuario == userId)
                .Include(g => g.Usuario);

            if (categoria.HasValue)
            {
                query = query.Where(g => g.Categorias == categoria.Value);
            }

            ViewBag.SubtotalGastos = await query.SumAsync(g => g.Valor ?? 0);
            ViewBag.SubtotalValorPagado = await query.SumAsync(g => g.ValorPagado);
            ViewBag.Total = ViewBag.SubtotalGastos - ViewBag.SubtotalValorPagado;
            ViewBag.NombreUsuario = nombreUsuario;
            ViewBag.Categorias = Enum.GetValues(typeof(Categoria))
                .Cast<Categoria>()
                .Select(c => new { Value = c, Text = c.ToString() });

            return View(await query.ToListAsync());
        }

        // GET: Gastos/CrearGasto
        public IActionResult CrearGasto()
        {
            var userId = GetUserIdFromCookies();
            if (userId == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            ViewBag.Categorias = Enum.GetValues(typeof(Categoria))
                .Cast<Categoria>()
                .Select(c => new { Value = c, Text = c.ToString() });

            return View(new Gasto
            {
                FechaRegristo = DateTime.Now,
                FechaFinal = DateTime.Now.AddDays(7),
                IdUsuario = userId.Value
            });
        }

        // POST: Gastos/CrearGasto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearGasto([Bind("IdGasto,IdUsuario,FechaRegristo,FechaFinal,Categorias,Descripcion,Valor,ValorPagado")] Gasto gasto)
        {
            var userId = GetUserIdFromCookies();
            if (userId == null || gasto.IdUsuario != userId)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            gasto.Estados = Estado.Pendiente;
            gasto.ValidarValor();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(gasto);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Gasto creado exitosamente!";
                    return RedirectToAction(nameof(PaginaInicial));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al crear el gasto: {ex.Message}");
                }
            }

            ViewBag.Categorias = Enum.GetValues(typeof(Categoria))
                .Cast<Categoria>()
                .Select(c => new { Value = c, Text = c.ToString() });

            return View(gasto);
        }

        // GET: Gastos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = GetUserIdFromCookies();
            if (userId == null || id == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var gasto = await _context.Gasto
                .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

            if (gasto == null)
            {
                return NotFound();
            }

            ViewBag.Categorias = Enum.GetValues(typeof(Categoria))
                .Cast<Categoria>()
                .Select(c => new { Value = c, Text = c.ToString() });

            return View(gasto);
        }

        // POST: Gastos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdGasto,IdUsuario,FechaRegristo,FechaFinal,Categorias,Descripcion,Valor,ValorPagado,Estados")] Gasto gasto)
        {
            var userId = GetUserIdFromCookies();
            if (userId == null || id != gasto.IdGasto || gasto.IdUsuario != userId)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    gasto.ValidarValor();
                    _context.Update(gasto);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Gasto actualizado correctamente!";
                    return RedirectToAction(nameof(PaginaInicial));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GastoExists(gasto.IdGasto))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                }
            }

            ViewBag.Categorias = Enum.GetValues(typeof(Categoria))
                .Cast<Categoria>()
                .Select(c => new { Value = c, Text = c.ToString() });

            return View(gasto);
        }

        // GET: Gastos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var userId = GetUserIdFromCookies();
            if (userId == null || id == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var gasto = await _context.Gasto
                .Include(g => g.Usuario)
                .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

            if (gasto == null)
            {
                return NotFound();
            }

            return View(gasto);
        }

        // POST: Gastos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserIdFromCookies();
            if (userId == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var gasto = await _context.Gasto
                .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

            if (gasto != null)
            {
                try
                {
                    _context.Gasto.Remove(gasto);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Gasto eliminado correctamente!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error al eliminar: {ex.Message}";
                }
            }
            return RedirectToAction(nameof(PaginaInicial));
        }

        // GET: Gastos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var userId = GetUserIdFromCookies();
            if (userId == null || id == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var gasto = await _context.Gasto
                .Include(g => g.Usuario)
                .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

            if (gasto == null)
            {
                return NotFound();
            }

            return View(gasto);
        }

        private bool GastoExists(int id)
        {
            return _context.Gasto.Any(e => e.IdGasto == id);
        }

        private int? GetUserIdFromCookies()
        {
            if (Request.Cookies.TryGetValue("UserId", out string userIdEncrypted))
            {
                try
                {
                    string userIdStr = _protector.Unprotect(userIdEncrypted);
                    if (int.TryParse(userIdStr, out int userId))
                    {
                        return userId;
                    }
                }
                catch
                {
                    Response.Cookies.Delete("UserId");
                    Response.Cookies.Delete("NombreUsuario");
                }
            }
            return null;
        }
    }
}