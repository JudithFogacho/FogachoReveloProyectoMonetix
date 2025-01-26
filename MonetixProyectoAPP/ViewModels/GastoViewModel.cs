using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System.Collections.ObjectModel;

namespace MonetixProyectoAPP.ViewModels
{
    public class GastoViewModel : BaseViewModel
    {
        private readonly GastoService _gastoService;
        private ObservableCollection<GastoResponse> _gastos = new();
        private ResumenGastos _resumen;

        public ObservableCollection<GastoResponse> Gastos
        {
            get => _gastos;
            set => SetProperty(ref _gastos, value);
        }

        public ResumenGastos Resumen
        {
            get => _resumen;
            set => SetProperty(ref _resumen, value);
        }

        public GastoViewModel(GastoService gastoService)
        {
            _gastoService = gastoService;
            LoadGastos();
        }

        private async Task LoadGastos()
        {
            await ExecuteAsync(async () =>
            {
                var gastos = await _gastoService.GetGastosAsync();
                Gastos = new ObservableCollection<GastoResponse>(gastos);
                Resumen = await _gastoService.GetResumenGastosAsync();
            });
        }

        public async Task CreateGastoAsync(
            DateTime fechaRegistro,
            DateTime fechaFinal,
            string categoria,
            string descripcion,
            double valor)
        {
            await ExecuteAsync(async () =>
            {
                await _gastoService.CreateGastoAsync(fechaRegistro, fechaFinal, categoria, descripcion, valor);
                await LoadGastos();
            });
        }

        public async Task DeleteGastoAsync(int idGasto)
        {
            await ExecuteAsync(async () =>
            {
                await _gastoService.DeleteGastoAsync(idGasto);
                await LoadGastos();
            });
        }

        public async Task PagarGastoAsync(int idGasto, double valorPago)
        {
            await ExecuteAsync(async () =>
            {
                await _gastoService.PagarGastoAsync(idGasto, valorPago);
                await LoadGastos();
            });
        }

        public async Task UpdateGastoAsync(
            int idGasto,
            DateTime? fechaRegistro = null,
            DateTime? fechaFinal = null,
            Categoria? categoria = null,
            string? descripcion = null,
            double? valor = null)
        {
            await ExecuteAsync(async () =>
            {
                await _gastoService.UpdateGastoAsync(idGasto, fechaRegistro, fechaFinal, categoria, descripcion, valor);
                await LoadGastos();
            });
        }
    }
}