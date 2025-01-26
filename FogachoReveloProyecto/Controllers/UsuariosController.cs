using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FogachoReveloProyecto.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FogachoReveloProyecto.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly FogachoReveloDataBase _context;
        private readonly IDataProtector _protector;

        public UsuariosController(FogachoReveloDataBase context, IDataProtectionProvider provider)
        {
            _context = context;
            _protector = provider.CreateProtector("Monetix.Usuario");
        }

        // GET: Usuarios/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Usuarios/Login
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
                .FirstOrDefaultAsync(u => u.Email == usuario.Email);

            if (userBaseDatos == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(usuario);
            }

            if (userBaseDatos.Password != usuario.Password)
            {
                ModelState.AddModelError(string.Empty, "Contraseña incorrecta.");
                return View(usuario);
            }

            // Configurar cookies seguras
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddMinutes(30)
            };

            Response.Cookies.Append("UserId", _protector.Protect(userBaseDatos.IdUsuario.ToString()), cookieOptions);
            Response.Cookies.Append("NombreUsuario", userBaseDatos.Nombre, cookieOptions);

            return RedirectToAction("PaginaInicial", "Gastos");
        }

        // GET: Usuarios/Registro
        public IActionResult Registro()
        {
            return View();
        }

        // POST: Usuarios/Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro([Bind("Nombre,Apellido,Email,Password")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Usuario.AnyAsync(u => u.Email == usuario.Email))
                {
                    ModelState.AddModelError("Email", "El correo electrónico ya está registrado.");
                    return View(usuario);
                }

                usuario.Gastos = new List<Gasto>();
                _context.Add(usuario);
                await _context.SaveChangesAsync();

                // Configurar cookies después del registro
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(30)
                };

                Response.Cookies.Append("UserId", _protector.Protect(usuario.IdUsuario.ToString()), cookieOptions);
                Response.Cookies.Append("NombreUsuario", usuario.Nombre, cookieOptions);

                return RedirectToAction("PaginaInicial", "Gastos");
            }
            return View(usuario);
        }

        // GET: Usuarios/Logout
        public IActionResult Logout()
        {
            // Eliminar cookies
            Response.Cookies.Delete("UserId");
            Response.Cookies.Delete("NombreUsuario");
            return RedirectToAction("Login", "Usuarios");
        }
    }
}