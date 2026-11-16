using System.Text.Json.Serialization;
using DemografiaLocalApp.Models.Serialization;

namespace DemografiaLocalApp.Models;

/// <summary>
/// Resposta da API de Nomes do Censo 2022 para uma localidade:
/// GET https://servicodados.ibge.gov.br/api/v3/nomes/2022/localidade/{idLocalidade}
///
/// Estrutura real (v3):
/// {
///   "localidade": 33,
///   "mulheres":  [ { "nome": "maria", "frequencia": 741049 }, ... ],
///   "homens":    [ { "nome": "jose",  "frequencia": 250172 }, ... ],
///   "sobrenomes":[ { "nome": "silva", "frequencia": 2515895 }, ... ]
/// }
///
/// O mesmo tipo também desserializa o formato "ranking" (v2), cujo elemento é
/// { "localidade": "33", "sexo": null, "res": [ { "nome": "MARIA", "frequencia": 752021, "ranking": 1 }, ... ] }.
/// </summary>
public sealed class LocalidadeNomesResposta
{
    /// <summary>
    /// Código IBGE da localidade. A API ora devolve número (v3), ora string (v2);
    /// o conversor abaixo aceita os dois formatos.
    /// </summary>
    [JsonPropertyName("localidade")]
    [JsonConverter(typeof(NumeroFlexivelInt32Converter))]
    public int Localidade { get; set; }

    /// <summary>Sexo do ranking (apenas no formato v2 com "res"); nulo = ambos.</summary>
    [JsonPropertyName("sexo")]
    public string? Sexo { get; set; }

    [JsonPropertyName("mulheres")]
    public List<ContagemNome> Mulheres
    {
        get => _mulheres;
        set => _mulheres = value ?? new();
    }

    [JsonPropertyName("homens")]
    public List<ContagemNome> Homens
    {
        get => _homens;
        set => _homens = value ?? new();
    }

    [JsonPropertyName("sobrenomes")]
    public List<ContagemNome> Sobrenomes
    {
        get => _sobrenomes;
        set => _sobrenomes = value ?? new();
    }

    /// <summary>
    /// Array interno do formato de ranking (v2). Fica vazio quando a resposta é a do v3.
    /// </summary>
    [JsonPropertyName("res")]
    public List<ContagemNome> Res
    {
        get => _res;
        set => _res = value ?? new();
    }

    private List<ContagemNome> _mulheres = new();
    private List<ContagemNome> _homens = new();
    private List<ContagemNome> _sobrenomes = new();
    private List<ContagemNome> _res = new();

    /// <summary>
    /// Lista combinada, usada para exibição quando a API devolve apenas "res".
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<ContagemNome> TodosOsNomes =>
        Mulheres.Count > 0 || Homens.Count > 0
            ? Mulheres.Concat(Homens).ToList()
            : Res;
}
