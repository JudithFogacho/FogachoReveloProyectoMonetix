using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonetixProyectoAPP.ViewModels
{
    public class GastoViewModel : BaseViewModel
    {
        private readonly GastoService _gastoService;
        private readonly UsuarioService _usuarioService;
        private readonly ApiPublicaService _apiPublicaService = new ApiPublicaService();

        private ObservableCollection<Local> _locales = new ObservableCollection<Local>();
        private ObservableCollection<Gasto> _gastos = new ObservableCollection<Gasto>();
        private int _currentUserId;

        public ObservableCollection<Gasto> Gastos
        {
            get => _gastos;
            set => SetProperty(ref _gastos, value);
        }

        public ObservableCollection<Local> Locales
        {
            get => _locales;
            set => SetProperty(ref _locales, value);
        }

        public GastoViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _gastoService = new GastoService(_usuarioService);
            _currentUserId = _usuarioService.GetCurrentUserId();
            LoadGastos();
        }

        private async Task LoadGastos()
        {
            if (_currentUserId == 0)
            {
                // Si no hay usuario autenticado, limpiar la lista
                Gastos.Clear();
                return;
            }

            await ExecuteAsync(async () => {
                var gastos = await _gastoService.GetGastosAsync();
                Gastos.Clear();
                foreach (var gasto in gastos)
                {
                    gasto.AsignarColorEstado();
                    Gastos.Add(gasto);
                }
            });
        }

        public async Task CargarLocalesPorCategoria(string categoria)
        {
            await ExecuteAsync(async () =>
            {
                var locales = await _apiPublicaService.GetLocalesPorCategoriaAsync(categoria);
                Locales.Clear();
                foreach (var local in locales)
                {
                    Locales.Add(local);
                }
            });
        }

        public async Task IngresarGasto(Gasto nuevoGasto)
        {
            if (_currentUserId == 0)
            {
                // Manejar el caso de usuario no autenticado
                return;
            }

            await ExecuteAsync(async () => {
                nuevoGasto.IdUsuario = _currentUserId;
                await _gastoService.CreateGastoAsync(nuevoGasto);
                await LoadGastos();
            });
        }

        public async Task EliminarGasto(int idGasto)
        {
            if (_currentUserId == 0)
            {
                return;
            }

            await ExecuteAsync(async () => {
                await _gastoService.DeleteGastoAsync(idGasto);
                await LoadGastos();
            });
        }

        public async Task PagarGasto(int idGasto, double valorPago)
        {
            if (_currentUserId == 0)
            {
                return;
            }

            await ExecuteAsync(async () => {
                var response = await _gastoService.PagarGastoAsync(idGasto, valorPago);
                if (response.IsSuccessStatusCode)
                {
                    await LoadGastos();
                }
            });
        }

        public async Task ActualizarGasto(Gasto gasto)
        {
            if (_currentUserId == 0)
            {
                return;
            }

            await ExecuteAsync(async () => {
                gasto.IdUsuario = _currentUserId;
                await _gastoService.UpdateGastoAsync(gasto);
                await LoadGastos();
            });
        }

        public void ActualizarUsuarioActual()
        {
            _currentUserId = _usuarioService.GetCurrentUserId();
            LoadGastos();
        }
    }
}