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
    public class GastoViewModel: INotifyPropertyChanged
    {
        private readonly GastoService _service = new GastoService();

        private List<Gasto> _gasto = new List<Gasto>();

        public List<Gasto> Gastos { get => _gasto; set { _gasto = value; OnPropertyChanged(); } }

        public GastoViewModel() { 
            LoadGastos ();
        
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged ([CallerMemberName] string propertyName = null) {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        private async Task LoadGastos()
        {
            var gastos = await _service.GetGastosAsync();
            Gastos.Clear ();
            foreach (var gasto in gastos)
            {
                Gastos.Add (gasto);
            }
            
        }

        public async Task IngresarGasto (Gasto nuevoGasto)
        {
            await _service.CreateGastoAsync(nuevoGasto);
            await LoadGastos();
        }

        public async Task EliminarGasto(int idGasto)
        {
            await _service.DeleteGastoAsync(idGasto);
            await LoadGastos();
        }


    }
}
