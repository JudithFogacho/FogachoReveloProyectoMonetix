using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonetixProyectoAPP.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged

    {
        private bool _estaOcupado;

        public bool EstaOcupado {
            get => _estaOcupado; 
            set {
                if (_estaOcupado != value)
                {
                    _estaOcupado = value;
                    OnPropertyChanged();
                }
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T> (ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;
            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;

        }

        protected async Task ExecuteAsync(Func<Task> task) {
            try
            {
                EstaOcupado = true;
                await task();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally { 
                EstaOcupado = false;
            }
        }

    }
}
