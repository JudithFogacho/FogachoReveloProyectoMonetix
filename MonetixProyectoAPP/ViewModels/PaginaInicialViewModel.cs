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
    public class PaginaInicialViewModel : INotifyPropertyChanged
    {

        private readonly GastoService _service = new GastoService();
       
        private List<Gasto> _gasto = new List<Gasto>();

        public List<Gasto> Gastos { get => _gasto; set { _gasto = value; OnPropertyChanged(); } }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        public PaginaInicialViewModel() {
            LoadPaginaInicial();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task LoadPaginaInicial()
        {
            var gastos = await _service.GetGastosAsync();
            Gastos.Clear();
            foreach (var gasto in gastos)
            {
                Gastos.Add(gasto);
            }

        }
    }
    
}
