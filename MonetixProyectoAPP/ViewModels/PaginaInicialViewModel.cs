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
using Microsoft.Maui.Controls;

namespace MonetixProyectoAPP.ViewModels
{
    public class PaginaInicialViewModel : BaseViewModel
    {
        private readonly GastoService _gastoService;
        private readonly UsuarioService _usuarioService;
        private ObservableCollection<Gasto> _gastos = new ObservableCollection<Gasto>();
        private ObservableCollection<Gasto> _gastosFiltrados = new ObservableCollection<Gasto>();
        private string _textoBusqueda;
        private int _currentUserId;

        public ObservableCollection<Gasto> Gastos
        {
            get => _gastos;
            set => SetProperty(ref _gastos, value);
        }

        public ObservableCollection<Gasto> GastosFiltrados
        {
            get => _gastosFiltrados;
            set
            {
                SetProperty(ref _gastosFiltrados, value);
                OnPropertyChanged(nameof(SubtotalGastos));
                OnPropertyChanged(nameof(SubtotalValorPagado));
                OnPropertyChanged(nameof(TotalGastos));
            }
        }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                SetProperty(ref _textoBusqueda, value);
                FiltrarGastos();
            }
        }

        public decimal SubtotalGastos => (decimal)(GastosFiltrados?.Sum(g => g.Valor) ?? 0);
        public decimal SubtotalValorPagado => (decimal)(GastosFiltrados?.Sum(g => g.ValorPagado) ?? 0);
        public decimal TotalGastos => SubtotalGastos - SubtotalValorPagado;

        public PaginaInicialViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _gastoService = new GastoService(_usuarioService);
            _currentUserId = _usuarioService.GetCurrentUserId();

            MessagingCenter.Subscribe<LoginViewModel, int>(this, "UserLoggedIn", (sender, userId) =>
            {
                _currentUserId = userId;
                CargarGastos();
            });

            MessagingCenter.Subscribe<LoginViewModel>(this, "UserLoggedOut", (sender) =>
            {
                _currentUserId = 0;
                Gastos.Clear();
                GastosFiltrados.Clear();
            });

            CargarGastos();
        }

        public async Task CargarGastos()
        {
            if (_currentUserId == 0)
            {
                await Shell.Current.GoToAsync("///Login");
                return;
            }

            await ExecuteAsync(async () =>
            {
                var gastos = await _gastoService.GetGastosAsync();
                if (gastos != null)
                {
                    Gastos.Clear();
                    foreach (var gasto in gastos)
                    {
                        gasto.AsignarColorEstado();
                        Gastos.Add(gasto);
                    }
                    GastosFiltrados = new ObservableCollection<Gasto>(Gastos);
                }
            });
        }

        private void FiltrarGastos()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                GastosFiltrados = new ObservableCollection<Gasto>(Gastos);
            }
            else
            {
                var textoBusquedaLower = TextoBusqueda.ToLower();
                GastosFiltrados = new ObservableCollection<Gasto>(
                    Gastos.Where(g =>
                        g.Categorias.ToString().ToLower().Contains(textoBusquedaLower) ||
                        g.Descripcion.ToLower().Contains(textoBusquedaLower)
                    )
                );
            }
        }

        public async Task NavegarADetalleGasto(Gasto gasto)
        {
            if (gasto != null && gasto.IdUsuario == _currentUserId)
            {
                await Shell.Current.GoToAsync($"DetalleGasto?gastoId={gasto.IdGasto}");
            }
        }

        public void Dispose()
        {
            MessagingCenter.Unsubscribe<LoginViewModel>(this, "UserLoggedOut");
            MessagingCenter.Unsubscribe<LoginViewModel, int>(this, "UserLoggedIn");
        }
    }
}