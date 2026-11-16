using System.Text.Json.Serialization;

namespace DemografiaLocalApp.Models;

/// <summary>
/// Município retornado por
/// GET https://servicodados.ibge.gov.br/api/v1/localidades/municipios
/// ou GET https://servicodados.ibge.gov.br/api/v1/localidades/estados/{uf}/municipios
///
/// Ex.:
/// {
///   "id": 3550308,
///   "nome": "São Paulo",
///   "microrregiao": {
///     "id": 35061,
///     "nome": "São Paulo",
///     "mesorregiao": {
///       "id": 3515,
///       "nome": "Metropolitana de São Paulo",
///       "UF": { "id": 35, "sigla": "SP", "nome": "São Paulo", "regiao": { ... } }
///     }
///   },
///   "regiao-imediata": {
///     "id": 350001,
///     "nome": "São Paulo",
///     "regiao-intermediaria": {
///       "id": 3501,
///       "nome": "São Paulo",
///       "UF": { ... }
///     }
///   }
/// }
/// </summary>
public sealed class Municipio
{
    /// <summary>Código IBGE do município (ex.: 3550308).</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("microrregiao")]
    public Microrregiao? Microrregiao { get; set; }

    [JsonPropertyName("regiao-imediata")]
    public RegiaoImediata? RegiaoImediata { get; set; }

    /// <summary>
    /// UF do município. O caminho "curto" (microrregiao → mesorregiao → UF) vem preenchido
    /// na listagem de municípios; o caminho por região imediata serve como alternativa.
    /// </summary>
    [JsonIgnore]
    public Uf? Uf => Microrregiao?.Mesorregiao?.Uf ?? RegiaoImediata?.RegiaoIntermediaria?.Uf;

    /// <summary>Texto exibido no Picker: "São Paulo - SP".</summary>
    [JsonIgnore]
    public string Descricao => Uf is null ? Nome : $"{Nome} - {Uf.Sigla}";
}

public sealed class Microrregiao
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("mesorregiao")]
    public Mesorregiao? Mesorregiao { get; set; }
}

public sealed class Mesorregiao
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("UF")]
    public Uf? Uf { get; set; }
}

/// <summary>
/// UF aninhada nos retornos de município (note as tags em maiúsculas: "UF").
/// </summary>
public sealed class Uf
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sigla")]
    public string Sigla { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("regiao")]
    public Regiao? Regiao { get; set; }

    [JsonIgnore]
    public string Descricao => $"{Nome} ({Sigla})";
}

public sealed class RegiaoImediata
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("regiao-intermediaria")]
    public RegiaoIntermediaria? RegiaoIntermediaria { get; set; }
}

public sealed class RegiaoIntermediaria
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("UF")]
    public Uf? Uf { get; set; }
}
