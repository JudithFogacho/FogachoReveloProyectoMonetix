using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonetixProyectoAPP.ViewModels
{
    public class RegistroViewModel
    {
        private readonly UsuarioService _service = new UsuarioService();

        private List<Usuario> _usuario = new List<Usuario>();
        public List<Usuario> Usuarios { get => _usuario; set { _usuario = value; OnPropertyChanged(); } }
        public RegistroViewModel()
        {
            LoadUsuarios();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        private async Task LoadUsuarios()
        {
            var usuarios = await _service.GetUsuariosAsync();
            Usuarios.Clear();
            foreach (var usuario in usuarios)
            {
                Usuarios.Add(usuario);
            }

        }
        public async Task IngresarUsuario(Usuario nuevoUsuario)
        {
            await _service.CreateUsuarioAsync(nuevoUsuario);
            await LoadUsuarios();
        }

        public async Task EliminarUsuario(int idUsuario)
        {
            await _service.DeleteUsuarioAsync(idUsuario);
            await LoadUsuarios();
        }
    }
}
