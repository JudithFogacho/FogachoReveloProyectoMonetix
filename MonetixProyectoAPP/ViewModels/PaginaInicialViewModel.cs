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
        private ObservableCollection<GastoResponse> _gastos = new();
        private ObservableCollection<GastoResponse> _gastosFiltrados = new();
        private string _textoBusqueda;

        public ObservableCollection<GastoResponse> Gastos
        {
            get => _gastos;
            set => SetProperty(ref _gastos, value);
        }

        public ObservableCollection<GastoResponse> GastosFiltrados
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

        public PaginaInicialViewModel(GastoService gastoService)
        {
            _gastoService = gastoService;
            CargarGastos();
        }

        public async Task CargarGastos()
        {
            await ExecuteAsync(async () =>
            {
                var gastos = await _gastoService.GetGastosAsync();
                Gastos = new ObservableCollection<GastoResponse>(gastos);
                GastosFiltrados = new ObservableCollection<GastoResponse>(gastos);
            });
        }

        private void FiltrarGastos()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                GastosFiltrados = new ObservableCollection<GastoResponse>(Gastos);
                return;
            }

            var textoBusquedaLower = TextoBusqueda.ToLower();
            GastosFiltrados = new ObservableCollection<GastoResponse>(
                Gastos.Where(g =>
                    g.Categoria.ToLower().Contains(textoBusquedaLower) ||
                    g.Descripcion.ToLower().Contains(textoBusquedaLower)
                )
            );
        }

        public async Task NavegarADetalleGasto(GastoResponse gasto)
        {
            if (gasto != null)
            {
                await Shell.Current.GoToAsync($"DetalleGasto?gastoId={gasto.IdGasto}");
            }
        }
    }
}