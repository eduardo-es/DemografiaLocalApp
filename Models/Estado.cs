using System.Text.Json.Serialization;

namespace DemografiaLocalApp.Models;

/// <summary>
/// Unidade da Federação (UF) retornada por
/// GET https://servicodados.ibge.gov.br/api/v1/localidades/estados
/// Ex.: { "id": 35, "sigla": "SP", "nome": "São Paulo", "regiao": { ... } }
/// </summary>
public sealed class Estado
{
    /// <summary>Código IBGE da UF (ex.: 35 para São Paulo).</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sigla")]
    public string Sigla { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("regiao")]
    public Regiao? Regiao { get; set; }

    /// <summary>Texto exibido no Picker: "São Paulo (SP)".</summary>
    [JsonIgnore]
    public string Descricao => $"{Nome} ({Sigla})";
}
