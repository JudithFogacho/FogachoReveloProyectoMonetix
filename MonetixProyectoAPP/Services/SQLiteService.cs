
using MonetixProyectoAPP.Models;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace MonetixProyectoAPP.Services
{

    public class SQLiteService
    {
        private readonly SQLiteAsyncConnection _database;

        public SQLiteService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Tienda>().Wait();
        }
        public async Task EliminarTiendaFavoritaAsync(Tienda tienda)
        {
            await _database.DeleteAsync(tienda);
        }

        public Task<List<Tienda>> GetTiendasFavoritasAsync()
        {
            return _database.Table<Tienda>().ToListAsync();
        }

        public async Task AddTiendaFavoritaAsync(Tienda tienda)
        {
            await _database.InsertAsync(tienda);
        }
    }
}
