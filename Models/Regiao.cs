using System.Text.Json.Serialization;

namespace DemografiaLocalApp.Models;

/// <summary>
/// Região do Brasil retornada pela API de Localidades do IBGE.
/// Ex.: { "id": 3, "sigla": "SE", "nome": "Sudeste" }
/// </summary>
public sealed class Regiao
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sigla")]
    public string Sigla { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonIgnore]
    public string Descricao => $"{Nome} ({Sigla})";
}
