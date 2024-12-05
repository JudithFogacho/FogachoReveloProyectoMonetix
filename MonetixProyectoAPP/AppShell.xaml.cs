namespace MonetixProyectoAPP
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("Login", typeof(MonetixProyectoAPP.Views.Login));
            Routing.RegisterRoute("Registro", typeof(MonetixProyectoAPP.Views.Registro));
        }
    }
}
