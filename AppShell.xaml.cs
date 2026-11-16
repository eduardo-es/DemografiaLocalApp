namespace DemografiaLocalApp
{
    public partial class AppShell : Shell
    {
        public AppShell(MainPage mainPage)
        {
            InitializeComponent();

            // Injeta a página já resolvida pelo container (com a ViewModel e os serviços).
            PaginaPrincipal.Content = mainPage;
        }
    }
}
