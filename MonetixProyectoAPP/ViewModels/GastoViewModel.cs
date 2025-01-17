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
    public class GastoViewModel: BaseViewModel
    {
        private readonly GastoService _gastoService = new GastoService();
        private readonly ApiPublicaService _apiPublicaService = new ApiPublicaService();

        private ObservableCollection<Local> _locales = new ObservableCollection<Local>();
        private ObservableCollection<Gasto> _gastos = new ObservableCollection<Gasto>();

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


        public GastoViewModel() { 
            LoadGastos();
        }

        private async Task LoadGastos()
        {
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

        public async Task IngresarGasto (Gasto nuevoGasto)
        {
            await ExecuteAsync(async () => {
                await _gastoService.CreateGastoAsync(nuevoGasto);
                await LoadGastos();
            });
        }

        public async Task EliminarGasto(int idGasto)
        {
            await ExecuteAsync(async () => {
                await _gastoService.DeleteGastoAsync(idGasto);
                await LoadGastos();
            });    
            
        }
    }
}
