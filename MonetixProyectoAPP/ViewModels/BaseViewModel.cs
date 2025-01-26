using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonetixProyectoAPP.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected async Task ExecuteAsync(Func<Task> action)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await action();
            }
            catch (Exception ex)
            {
                await HandleException(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task HandleException(Exception ex)
        {
            // Implementación específica para diferentes tipos de excepciones
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}