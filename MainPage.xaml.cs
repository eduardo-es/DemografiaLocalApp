using DemografiaLocalApp.ViewModels;

namespace DemografiaLocalApp;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Carrega os estados uma única vez; a consulta de nomes é disparada
        // automaticamente pela ViewModel quando o Picker muda de seleção.
        if (_viewModel.Estados.Count == 0)
            await _viewModel.CarregarEstadosCommand.ExecuteAsync(null);
    }
}
