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
        //declaracion de una variable privada de solo lectura
        private readonly FogachoReveloDataBase _context;

        public UsuariosController(FogachoReveloDataBase context)
        {
            _context = context;
        }

        // GET: Usuarios/Create
        //metodos de acción del controlador
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

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        //Este apartado es del login y nos ayuda a verificar que el acceso con contraseñas y email
        //esten dentro de la base de datos

        public async Task<IActionResult> Login([Bind("Email,Password")] Usuario usuario)
        {

            if (string.IsNullOrEmpty(usuario.Email) || string.IsNullOrEmpty(usuario.Password))
            {
                ModelState.AddModelError(string.Empty, "Email y contraseña son requeridos.");
                return View(usuario);
            }

            var userBaseDatos = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == usuario.Email);

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

            return RedirectToAction("PaginaInicial", "Gastos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //Metodo para el registro
        //Añade a un nuevo usuario y una vez añadido retorna al Login
        public async Task<IActionResult> Registro([Bind("IdUsuario,Nombre,Apellido,Email,Password")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(usuario);
        }
    }
}
