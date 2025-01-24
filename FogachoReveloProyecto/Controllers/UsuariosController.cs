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
    public class UsuariosController : Controller
    {
        private readonly FogachoReveloDataBase _context;

        public UsuariosController(FogachoReveloDataBase context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email,Password")] Usuario usuario)
        {
            if (string.IsNullOrEmpty(usuario.Email) || string.IsNullOrEmpty(usuario.Password))
            {
                ModelState.AddModelError(string.Empty, "Email y contraseña son requeridos.");
                return View(usuario);
            }

            var userBaseDatos = await _context.Usuario
                .Include(u => u.Gastos)
                .FirstOrDefaultAsync(u => u.Email == usuario.Email);

            if (userBaseDatos == null)
            {
                ModelState.AddModelError(string.Empty, "Email no encontrado.");
                return View(usuario);
            }

            if (userBaseDatos.Password != usuario.Password)
            {
                ModelState.AddModelError(string.Empty, "Contraseña incorrecta.");
                return View(usuario);
            }

            // Guardar el ID y el nombre del usuario en TempData
            TempData["UserId"] = userBaseDatos.IdUsuario;
            TempData["NombreUsuario"] = userBaseDatos.Nombre;

            // Redirigir a la página principal
            return RedirectToAction("PaginaInicial", "Gastos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro([Bind("IdUsuario,Nombre,Apellido,Email,Password")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // Verificar si el email ya existe
                var existingUser = await _context.Usuario
                    .FirstOrDefaultAsync(u => u.Email == usuario.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Este correo electrónico ya está registrado.");
                    return View(usuario);
                }

                // Inicializar la colección de Gastos
                usuario.Gastos = new List<Gasto>();

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(usuario);
        }
    }
}