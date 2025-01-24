using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;

namespace MonetixProyectoAPP.ViewModels
{
    public class TiendasFavoritasGuardadasViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqliteService;

        public ObservableCollection<Tienda> TiendasFavoritas { get; set; }

        public ICommand EliminarTiendaCommand { get; }

        public TiendasFavoritasGuardadasViewModel(SQLiteService sqliteService)
        {
            _sqliteService = sqliteService;
            TiendasFavoritas = new ObservableCollection<Tienda>();

            EliminarTiendaCommand = new Command<Tienda>(async (tienda) => await EliminarTiendaAsync(tienda));

            // Cargar las tiendas favoritas al inicializar el ViewModel
            LoadTiendasFavoritasAsync();
        }

        private async Task LoadTiendasFavoritasAsync()
        {
            var tiendasFavoritas = await _sqliteService.GetTiendasFavoritasAsync();
            foreach (var tienda in tiendasFavoritas)
            {
                TiendasFavoritas.Add(tienda);
            }
        }

        private async Task EliminarTiendaAsync(Tienda tienda)
        {
            await _sqliteService.EliminarTiendaFavoritaAsync(tienda);
            TiendasFavoritas.Remove(tienda);
        }
    }
}