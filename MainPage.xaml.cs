using DemografiaLocalApp.ViewModels;

namespace DemografiaLocalApp;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private bool _seletorAberto;

    private async void OnEscolherEstado(object? sender, EventArgs e)
    {
        if (_seletorAberto || !_viewModel.PodeSelecionarEstado) return;
        _seletorAberto = true;
        var pagina = new LocalidadeSelectorPage<Models.Estado>("Escolha o estado",
            "Busque por nome ou sigla", _viewModel.Estados.ToList(), item => item.Descricao,
            item => _viewModel.EstadoSelecionado = item);
        pagina.Disappearing += (_, _) => _seletorAberto = false;
        await Navigation.PushModalAsync(new NavigationPage(pagina));
    }

    private async void OnEscolherMunicipio(object? sender, EventArgs e)
    {
        if (_seletorAberto || !_viewModel.PodeSelecionarMunicipio) return;
        _seletorAberto = true;
        var pagina = new LocalidadeSelectorPage<Models.Municipio>(
            $"Municípios de {_viewModel.EstadoSelecionado!.Sigla}", "Busque pelo nome do município",
            _viewModel.Municipios.ToList(), item => item.Descricao,
            item => _viewModel.MunicipioSelecionado = item);
        pagina.Disappearing += (_, _) => _seletorAberto = false;
        await Navigation.PushModalAsync(new NavigationPage(pagina));
    }

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
