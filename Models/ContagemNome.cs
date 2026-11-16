using System.Globalization;
using System.Text.Json.Serialization;

namespace DemografiaLocalApp.Models;

/// <summary>
/// Um nome com a quantidade de pessoas que o possuem em uma localidade.
///
/// Formato retornado pela API de Nomes 2022 (v3):
/// { "nome": "maria", "frequencia": 741049 }
///
/// Formato do ranking (v2), que traz o array "res":
/// { "nome": "MARIA", "frequencia": 752021, "ranking": 1 }
/// </summary>
public sealed class ContagemNome
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Quantidade de pessoas registradas com o nome na localidade.</summary>
    [JsonPropertyName("frequencia")]
    public long Frequencia { get; set; }

    /// <summary>Posição no ranking (presente apenas no formato com "res").</summary>
    [JsonPropertyName("ranking")]
    public int? Ranking { get; set; }

    /// <summary>Proporção do nome na população (quando a API a retorna).</summary>
    [JsonPropertyName("proporcao")]
    [JsonConverter(typeof(Serialization.NumeroFlexivelDoubleConverter))]
    public double? Proporcao { get; set; }

    /// <summary>Sexo associado ao nome: "M", "F" ou nulo (ambos).</summary>
    [JsonPropertyName("sexo")]
    public string? Sexo { get; set; }

    [JsonIgnore]
    public bool EhMasculino => string.Equals(Sexo, "M", StringComparison.OrdinalIgnoreCase);

    [JsonIgnore]
    public bool EhFeminino => string.Equals(Sexo, "F", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Nome pronto para exibição: a API devolve tudo em minúsculas ("maria"),
    /// aqui vira "Maria".
    /// </summary>
    [JsonIgnore]
    public string NomeExibicao => string.IsNullOrWhiteSpace(Nome)
        ? string.Empty
        : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Nome.ToLower(CultureInfo.CurrentCulture));
}
