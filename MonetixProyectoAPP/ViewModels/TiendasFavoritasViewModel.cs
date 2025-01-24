using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;

namespace MonetixProyectoAPP.ViewModels
{
    public class TiendasFavoritasViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqliteService;
        private readonly ApiPublicaService _apiPublicaService;

        public ObservableCollection<Tienda> Tiendas { get; set; }

        public ICommand AddTiendaFavoritaCommand { get; }

        public TiendasFavoritasViewModel(SQLiteService sqliteService, ApiPublicaService apiPublicaService)
        {
            _sqliteService = sqliteService;
            _apiPublicaService = apiPublicaService;
            Tiendas = new ObservableCollection<Tienda>();

            AddTiendaFavoritaCommand = new Command<Tienda>(async (tienda) => await AddTiendaFavoritaAsync(tienda));

            // Cargar las tiendas al inicializar el ViewModel
            LoadTiendasAsync();
        }

        private async Task LoadTiendasAsync()
        {
            var tiendas = await _apiPublicaService.GetTiendasAsync();
            foreach (var tienda in tiendas)
            {
                Tiendas.Add(tienda);
            }
        }

        private async Task AddTiendaFavoritaAsync(Tienda tienda)
        {
            try
            {
                // Intenta agregar la tienda
                await _sqliteService.AddTiendaFavoritaAsync(tienda);

                // Muestra un mensaje de éxito
                await Shell.Current.DisplayAlert("Éxito", "Tienda añadida a favoritos", "OK");

                // Regresa a la página anterior
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                // Muestra el error en un diálogo de alerta
                await Shell.Current.DisplayAlert("Error", $"No se pudo añadir la tienda: {ex.Message}", "OK");

                // También puedes registrar el error en un archivo de log si lo deseas
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}