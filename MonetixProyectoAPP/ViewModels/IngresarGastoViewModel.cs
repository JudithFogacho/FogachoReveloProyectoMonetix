using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonetixProyectoAPP.ViewModels
{
    public class IngresarGastoViewModel : BaseViewModel
    {
        private readonly GastoService _gastoService = new GastoService();
        
        public List<string> Categorias { get; set; }
        private DateTime _fechaRegistro = DateTime.Now;

        public DateTime FechaRegistro { get => _fechaRegistro; set => SetProperty(ref _fechaRegistro, value); }

        private DateTime _fechaFinal = DateTime.Now;

        public DateTime FechaFinal { get => _fechaFinal; set => SetProperty(ref _fechaFinal, value); }

        private string _categoriaSeleccionada;
        public string CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set => SetProperty(ref _categoriaSeleccionada, value);
        }

        private string _descripcion;
        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        private double _valor;

        public double Valor { 
            get => _valor;
            set => SetProperty(ref _valor, value);
        }

        public ICommand GuardarGastoCommand { get; }
        public IngresarGastoViewModel()
        {
            CargarCategorias();
            GuardarGastoCommand = new Command(async () => await GuardarGastoAsync());

        }

        private void CargarCategorias(){
            Categorias = new List<string> { 
                "Entretenimiento",
                "Comida",
                "Transporte",
                "Ropa",
                "Educacion",
                "Salud",
                "ServiciosBasicos",
                "Otro"
            };
        }

        private async Task GuardarGastoAsync()
        {
            if (string.IsNullOrEmpty(CategoriaSeleccionada) ||
                string.IsNullOrEmpty(Descripcion) ||
                Valor == 0) {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, completa todos los campos correctamente.", "OK");
                return;
            }

            var nuevoGasto = new Gasto
            {
                FechaRegristo = FechaRegistro,
                FechaFinal = FechaFinal,
                Categorias = Enum.TryParse(CategoriaSeleccionada, out Categoria categoria)
                    ? categoria : Categoria.Otro,
                Descripcion = Descripcion,
                Valor = Valor,
                ValorPagado =0
            };

            nuevoGasto.ValidarValor();
            nuevoGasto.AsignarColorEstado();

            await _gastoService.CreateGastoAsync(nuevoGasto);

            await Application.Current.MainPage.DisplayAlert("Exito", "El gasto se ha registrado correctamente.", "OK");
            await Shell.Current.GoToAsync("///PaginaInical");
        }
    }
}
