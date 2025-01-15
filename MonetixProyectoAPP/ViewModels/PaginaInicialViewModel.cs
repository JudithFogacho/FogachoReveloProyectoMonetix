using Kotlin.Properties;
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
    public class PaginaInicialViewModel : BaseViewModel
    {

        private readonly GastoService _gastoService = new GastoService();

        private ObservableCollection<Gasto> _gastos = new ObservableCollection<Gasto>();
        private ObservableCollection<Gasto> _gastosFiltrados = new ObservableCollection<Gasto>();

        private string _textoBusqueda;

        public ObservableCollection<Gasto> Gastos { get => _gastos; 
            set => SetProperty(ref _gastos, value);
        }

       public ObservableCollection<Gasto> GastosFiltrados
        {
            get => _gastosFiltrados;
            set { 
                SetProperty(ref _gastosFiltrados, value);
                OnPropertyChanged(nameof(SubtotalGastos);
                OnPropertyChanged(nameof(SubtotalValorPagado);
                OnPropertyChanged(nameof(TotalGastos);
            }
        }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set { 
                SetProperty(ref _textoBusqueda, value);
                FiltradosGastos();
            }

        }

        public decimal SubtotalGastos => (decimal)GastosFiltrados.Sum(g => g.Valor);

        public decimal SubtotalValorPagado => (decimal)GastosFiltrados.Sum(g => g.ValorPagado);
        public decimal TotalGastos => SubtotalGastos - SubtotalValorPagado;
        public PaginaInicialViewModel() {
            CargarGastos();
        }

        private async Task CargarGastos()
        {
            await ExecuteAsync(async () => {
                var gastos = await _gastoService.GetGastosAsync();
                if (gastos != null) {
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

        private void FiltrarGastos() {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                GastosFiltrados = new ObservableCollection<Gasto>(Gastos);
            }
            else { 
                var textoBusquedaLower = TextoBusqueda.ToLower();
                GastosFiltrados = new ObservableCollection<Gasto>(
                    Gastos.Where(g => g.Categorias.ToString().ToLower().Contains(textoBusquedaLower))
                    );
            }
        }

        public async Task NavegarADetalleGastO (Gasto gasto)
        {
            if (gasto != null) {
                await Shell.Current.GoToAsync($"DetalleGasto?gastoId={gasto.IdGasto}");
            }
    }
    
}
