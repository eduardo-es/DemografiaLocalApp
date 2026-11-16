using DemografiaLocalApp.Models;

namespace DemografiaLocalApp.Services;

/// <summary>
/// Contrato de acesso às APIs públicas do IBGE.
/// Registrado no container de DI e consumido pelas telas (UI) via injeção de construtor.
/// </summary>
public interface IIbgeApiService
{
    /// <summary>
    /// Lista as Unidades da Federação.
    /// GET https://servicodados.ibge.gov.br/api/v1/localidades/estados
    /// </summary>
    Task<IReadOnlyList<Estado>> ObterEstadosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista os municípios de um estado.
    /// GET https://servicodados.ibge.gov.br/api/v1/localidades/estados/{estadoId}/municipios
    /// </summary>
    Task<IReadOnlyList<Municipio>> ObterMunicipiosPorEstadoAsync(
        int estadoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca os nomes mais comuns (homens, mulheres e sobrenomes) de uma localidade no Censo 2022.
    /// GET https://servicodados.ibge.gov.br/api/v3/nomes/2022/localidade/{idLocalidade}
    /// </summary>
    Task<LocalidadeNomesResposta> ObterNomesPorLocalidadeAsync(
        int idLocalidade,
        CancellationToken cancellationToken = default);
}
