using System.Text.Json;
using DemografiaLocalApp.Models;

namespace DemografiaLocalApp.Services;

/// <summary>
/// Cliente HTTP das APIs públicas do IBGE.
/// Não cria o <see cref="HttpClient"/>: ele é injetado pelo container de DI,
/// o que permite reutilizar conexões e configurar BaseAddress/Timeout em um único lugar.
/// </summary>
public sealed class IbgeApiService : IIbgeApiService
{
    /// <summary>Endereço base da API do IBGE (usado quando o HttpClient não define BaseAddress).</summary>
    public const string UrlBasePadrao = "https://servicodados.ibge.gov.br/api/";

    private static readonly Uri UrlBaseFallback = new(UrlBasePadrao);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public IbgeApiService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    private Uri BaseAddress
    {
        get
        {
            var endereco = _httpClient.BaseAddress ?? UrlBaseFallback;

            // Sem a barra final, "new Uri(base, "v1/...")" descartaria o último segmento.
            return endereco.AbsoluteUri.EndsWith('/')
                ? endereco
                : new Uri(endereco.AbsoluteUri + "/");
        }
    }

    public Task<IReadOnlyList<Estado>> ObterEstadosAsync(CancellationToken cancellationToken = default)
        => ObterListaAsync<Estado>("v1/localidades/estados", cancellationToken);

    public Task<IReadOnlyList<Municipio>> ObterMunicipiosPorEstadoAsync(
        int estadoId,
        CancellationToken cancellationToken = default)
    {
        if (estadoId <= 0)
            throw new ArgumentOutOfRangeException(nameof(estadoId), "O código do estado deve ser maior que zero.");

        return ObterListaAsync<Municipio>($"v1/localidades/estados/{estadoId}/municipios", cancellationToken);
    }

    public async Task<LocalidadeNomesResposta> ObterNomesPorLocalidadeAsync(
        int idLocalidade,
        CancellationToken cancellationToken = default)
    {
        if (idLocalidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(idLocalidade), "O código da localidade deve ser maior que zero.");

        var resposta = await ObterJsonAsync<LocalidadeNomesResposta>(
            $"v3/nomes/2022/localidade/{idLocalidade}", cancellationToken).ConfigureAwait(false);

        return resposta;
    }

    private async Task<IReadOnlyList<T>> ObterListaAsync<T>(string caminho, CancellationToken cancellationToken)
    {
        var lista = await ObterJsonAsync<List<T>>(caminho, cancellationToken).ConfigureAwait(false);
        return lista;
    }

    /// <summary>
    /// Executa o GET, valida o status e desserializa o JSON, traduzindo falhas
    /// de rede/HTTP/formato em <see cref="IbgeApiException"/> com mensagem amigável.
    /// </summary>
    private async Task<T> ObterJsonAsync<T>(string caminho, CancellationToken cancellationToken)
    {
        var url = new Uri(BaseAddress, caminho);

        try
        {
            using var resposta = await _httpClient
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!resposta.IsSuccessStatusCode)
            {
                throw new IbgeApiException(
                    $"A API do IBGE respondeu com erro {(int)resposta.StatusCode} ({resposta.ReasonPhrase}). " +
                    "Tente novamente em alguns instantes.")
                {
                    StatusCode = (int)resposta.StatusCode
                };
            }

            await using var conteudo = await resposta.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            var dados = await JsonSerializer
                .DeserializeAsync<T>(conteudo, JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            return dados ?? throw new IbgeApiException("A API do IBGE retornou uma resposta vazia.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout do HttpClient (não foi o usuário/navegação que cancelou).
            throw new IbgeApiException(
                "A consulta ao IBGE demorou demais para responder. Verifique sua conexão e tente novamente.");
        }
        catch (HttpRequestException ex)
        {
            throw new IbgeApiException(
                "Não foi possível conectar ao IBGE. Verifique sua conexão com a internet e tente novamente.", ex);
        }
        catch (JsonException ex)
        {
            throw new IbgeApiException(
                "O IBGE retornou dados em um formato inesperado. Tente novamente mais tarde.", ex);
        }
    }
}
