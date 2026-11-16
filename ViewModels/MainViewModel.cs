using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DemografiaLocalApp.Models;
using DemografiaLocalApp.Services;

namespace DemografiaLocalApp.ViewModels;

/// <summary>Tipo de localidade usada na consulta de nomes.</summary>
public enum TipoLocalidade
{
    Estado,
    Municipio
}

/// <summary>
/// ViewModel da tela principal. Faz a ponte entre a UI e a camada de Serviços:
/// carrega estados/municípios, reage à troca de localidade e separa os nomes
/// retornados pelo IBGE nas listas de homens, mulheres e sobrenomes.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IIbgeApiService _ibge;
    private CancellationTokenSource? _ctsConsultaNomes;
    private CancellationTokenSource? _ctsConsultaMunicipios;
    private bool _ignorarMudancaDeMunicipio;

    public MainViewModel(IIbgeApiService ibge)
    {
        _ibge = ibge ?? throw new ArgumentNullException(nameof(ibge));
    }

    // ---------- Coleções ligadas à interface ----------

    public ObservableCollection<Estado> Estados { get; } = new();

    public ObservableCollection<Municipio> Municipios { get; } = new();

    public ObservableCollection<ContagemNome> NomesHomens { get; } = new();

    public ObservableCollection<ContagemNome> NomesMulheres { get; } = new();

    public ObservableCollection<ContagemNome> Sobrenomes { get; } = new();

    // ---------- Seleção e modo ----------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModoEstado))]
    [NotifyPropertyChangedFor(nameof(ModoMunicipio))]
    [NotifyPropertyChangedFor(nameof(TemLocalidadeSelecionada))]
    [NotifyPropertyChangedFor(nameof(LocalidadeDescricao))]
    [NotifyPropertyChangedFor(nameof(TextoDicaInicial))]
    [NotifyPropertyChangedFor(nameof(MostrarDicaInicial))]
    [NotifyPropertyChangedFor(nameof(MostrarSemResultados))]
    public partial TipoLocalidade Modo { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemLocalidadeSelecionada))]
    [NotifyPropertyChangedFor(nameof(LocalidadeDescricao))]
    [NotifyPropertyChangedFor(nameof(TextoDicaInicial))]
    [NotifyPropertyChangedFor(nameof(MostrarDicaInicial))]
    [NotifyPropertyChangedFor(nameof(MostrarSemResultados))]
    public partial Estado? EstadoSelecionado { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemLocalidadeSelecionada))]
    [NotifyPropertyChangedFor(nameof(LocalidadeDescricao))]
    [NotifyPropertyChangedFor(nameof(MostrarDicaInicial))]
    [NotifyPropertyChangedFor(nameof(MostrarSemResultados))]
    public partial Municipio? MunicipioSelecionado { get; set; }

    // ---------- Estado da tela ----------

    [ObservableProperty]
    public partial bool EstaCarregandoEstados { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarDicaInicial))]
    [NotifyPropertyChangedFor(nameof(MostrarSemResultados))]
    public partial bool EstaCarregandoMunicipios { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarDicaInicial))]
    [NotifyPropertyChangedFor(nameof(MostrarSemResultados))]
    public partial bool EstaCarregandoNomes { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarDicaInicial))]
    [NotifyPropertyChangedFor(nameof(MostrarSemResultados))]
    public partial bool TemNomes { get; set; }

    [ObservableProperty]
    public partial bool TemSobrenomes { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemErro))]
    [NotifyPropertyChangedFor(nameof(MostrarDicaInicial))]
    [NotifyPropertyChangedFor(nameof(MostrarSemResultados))]
    public partial string? MensagemErro { get; set; }

    // ---------- Propriedades calculadas ----------

    public bool ModoEstado => Modo == TipoLocalidade.Estado;

    public bool ModoMunicipio => Modo == TipoLocalidade.Municipio;

    public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);

    public bool TemLocalidadeSelecionada => Modo == TipoLocalidade.Municipio
        ? MunicipioSelecionado is not null
        : EstadoSelecionado is not null;

    /// <summary>Dica exibida antes de o usuário escolher a localidade.</summary>
    public bool MostrarDicaInicial =>
        !TemLocalidadeSelecionada &&
        !TemNomes &&
        !EstaCarregandoNomes &&
        !EstaCarregandoMunicipios &&
        !TemErro;

    /// <summary>Localidade válida, consulta encerrada e nenhum nome retornado.</summary>
    public bool MostrarSemResultados =>
        TemLocalidadeSelecionada &&
        !TemNomes &&
        !EstaCarregandoNomes &&
        !EstaCarregandoMunicipios &&
        !TemErro;

    public string LocalidadeDescricao => Modo == TipoLocalidade.Municipio
        ? MunicipioSelecionado?.Descricao ?? "Selecione um município"
        : EstadoSelecionado?.Descricao ?? "Selecione um estado";

    public string TextoDicaInicial => Modo switch
    {
        TipoLocalidade.Municipio when EstadoSelecionado is null =>
            "Escolha um estado e, em seguida, um município para consultar os nomes mais comuns.",
        TipoLocalidade.Municipio =>
            "Agora escolha um município para consultar os nomes mais comuns.",
        _ =>
            "Escolha um estado para consultar os nomes mais comuns."
    };

    // ---------- Comandos ----------

    /// <summary>Carrega a lista de estados. Chamado pela página no OnAppearing.</summary>
    [RelayCommand]
    private async Task CarregarEstadosAsync(CancellationToken cancellationToken)
    {
        if (EstaCarregandoEstados)
            return;

        EstaCarregandoEstados = true;
        MensagemErro = null;

        try
        {
            var estados = await _ibge.ObterEstadosAsync(cancellationToken);
            var idSelecionado = EstadoSelecionado?.Id;

            Estados.Clear();
            foreach (var estado in estados.OrderBy(e => e.Nome, StringComparer.CurrentCulture))
                Estados.Add(estado);

            if (idSelecionado is not null)
                EstadoSelecionado = Estados.FirstOrDefault(e => e.Id == idSelecionado);
        }
        catch (OperationCanceledException)
        {
            // Navegação/página fechada: não é um erro para o usuário.
        }
        catch (IbgeApiException ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            EstaCarregandoEstados = false;
        }
    }

    [RelayCommand]
    private void SelecionarModoEstado() => MudarModo(TipoLocalidade.Estado);

    [RelayCommand]
    private void SelecionarModoMunicipio() => MudarModo(TipoLocalidade.Municipio);

    /// <summary>
    /// Carrega os municípios do estado selecionado. Ao entrar no modo município,
    /// os nomes antigos são limpos e a consulta de nomes só ocorre após escolher um município.
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task CarregarMunicipiosAsync()
    {
        var estado = EstadoSelecionado;

        CancelarConsultaMunicipios();
        CancelarConsultaNomes();
        EstaCarregandoNomes = false;
        SubstituirNomes(null);
        MensagemErro = null;
        Municipios.Clear();
        DefinirMunicipioSelecionadoSemConsulta(null);

        if (estado is null)
        {
            EstaCarregandoMunicipios = false;
            return;
        }

        var cts = new CancellationTokenSource();
        _ctsConsultaMunicipios = cts;
        EstaCarregandoMunicipios = true;

        try
        {
            var municipios = await _ibge.ObterMunicipiosPorEstadoAsync(estado.Id, cts.Token);
            cts.Token.ThrowIfCancellationRequested();

            foreach (var municipio in municipios.OrderBy(m => m.Nome, StringComparer.CurrentCulture))
                Municipios.Add(municipio);
        }
        catch (OperationCanceledException)
        {
            // Outro estado foi selecionado antes desta resposta chegar.
        }
        catch (IbgeApiException ex)
        {
            if (!cts.IsCancellationRequested)
                MensagemErro = ex.Message;
        }
        finally
        {
            var cancelada = cts.IsCancellationRequested;

            if (ReferenceEquals(_ctsConsultaMunicipios, cts))
                _ctsConsultaMunicipios = null;

            if (!cancelada)
                EstaCarregandoMunicipios = false;

            cts.Dispose();
        }
    }

    /// <summary>
    /// Consulta a API de Nomes para a localidade do modo atual e distribui
    /// o resultado nas listas. Disparado automaticamente pelas seleções.
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task CarregarNomesAsync()
    {
        var localidadeId = Modo == TipoLocalidade.Municipio
            ? MunicipioSelecionado?.Id
            : EstadoSelecionado?.Id;

        CancelarConsultaNomes();
        SubstituirNomes(null);
        MensagemErro = null;

        if (localidadeId is null)
        {
            EstaCarregandoNomes = false;
            return;
        }

        var cts = new CancellationTokenSource();
        _ctsConsultaNomes = cts;
        EstaCarregandoNomes = true;

        try
        {
            var dados = await _ibge.ObterNomesPorLocalidadeAsync(localidadeId.Value, cts.Token);
            cts.Token.ThrowIfCancellationRequested();

            SubstituirNomes(dados);
        }
        catch (OperationCanceledException)
        {
            // Outra localidade foi selecionada antes desta resposta chegar.
        }
        catch (IbgeApiException ex)
        {
            if (!cts.IsCancellationRequested)
                MensagemErro = ex.Message;
        }
        finally
        {
            var cancelada = cts.IsCancellationRequested;

            if (ReferenceEquals(_ctsConsultaNomes, cts))
                _ctsConsultaNomes = null;

            if (!cancelada)
                EstaCarregandoNomes = false;

            cts.Dispose();
        }
    }

    /// <summary>Repete a última etapa que falhou (estados, municípios ou nomes).</summary>
    [RelayCommand]
    private async Task TentarNovamenteAsync()
    {
        if (Estados.Count == 0)
        {
            await CarregarEstadosAsync(CancellationToken.None);
            return;
        }

        if (Modo == TipoLocalidade.Municipio && MunicipioSelecionado is null)
        {
            await CarregarMunicipiosAsync();
            return;
        }

        await CarregarNomesAsync();
    }

    // ---------- Reações às mudanças ----------

    partial void OnModoChanged(TipoLocalidade value)
    {
        if (value == TipoLocalidade.Estado)
        {
            CancelarConsultaMunicipios();
            EstaCarregandoMunicipios = false;
            Municipios.Clear();
            DefinirMunicipioSelecionadoSemConsulta(null);

            _ = CarregarNomesCommand.ExecuteAsync(null);
            return;
        }

        _ = CarregarMunicipiosCommand.ExecuteAsync(null);
    }

    partial void OnEstadoSelecionadoChanged(Estado? value)
    {
        if (Modo == TipoLocalidade.Municipio)
            _ = CarregarMunicipiosCommand.ExecuteAsync(null);
        else
            _ = CarregarNomesCommand.ExecuteAsync(null);
    }

    partial void OnMunicipioSelecionadoChanged(Municipio? value)
    {
        // Ignora limpezas internas e deseleções; só consulta ao escolher um município.
        if (_ignorarMudancaDeMunicipio || Modo != TipoLocalidade.Municipio || value is null)
            return;

        _ = CarregarNomesCommand.ExecuteAsync(null);
    }

    // ---------- Apoio ----------

    private void MudarModo(TipoLocalidade novoModo)
    {
        if (Modo == novoModo)
            return;

        Modo = novoModo;
    }

    private void SubstituirNomes(LocalidadeNomesResposta? dados)
    {
        NomesHomens.Clear();
        NomesMulheres.Clear();
        Sobrenomes.Clear();

        if (dados is not null)
        {
            foreach (var nome in dados.Homens.OrderByDescending(n => n.Frequencia))
                NomesHomens.Add(nome);

            foreach (var nome in dados.Mulheres.OrderByDescending(n => n.Frequencia))
                NomesMulheres.Add(nome);

            foreach (var nome in dados.Sobrenomes.OrderByDescending(n => n.Frequencia))
                Sobrenomes.Add(nome);
        }

        TemNomes = NomesHomens.Count > 0 || NomesMulheres.Count > 0;
        TemSobrenomes = TemNomes && Sobrenomes.Count > 0;
    }

    private void DefinirMunicipioSelecionadoSemConsulta(Municipio? municipio)
    {
        _ignorarMudancaDeMunicipio = true;
        try
        {
            MunicipioSelecionado = municipio;
        }
        finally
        {
            _ignorarMudancaDeMunicipio = false;
        }
    }

    private void CancelarConsultaNomes()
    {
        var anterior = _ctsConsultaNomes;
        _ctsConsultaNomes = null;

        if (anterior is null)
            return;

        try
        {
            anterior.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // A consulta anterior já terminou e descartou o token.
        }
    }

    private void CancelarConsultaMunicipios()
    {
        var anterior = _ctsConsultaMunicipios;
        _ctsConsultaMunicipios = null;

        if (anterior is null)
            return;

        try
        {
            anterior.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // A consulta anterior já terminou e descartou o token.
        }
    }
}
