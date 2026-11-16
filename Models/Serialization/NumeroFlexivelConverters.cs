using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DemografiaLocalApp.Models.Serialization;

/// <summary>
/// Aceita um número JSON tanto como valor numérico (33) quanto como texto ("33").
/// A API v3 devolve "localidade" como número e a API de ranking (v2) como string.
/// </summary>
public sealed class NumeroFlexivelInt32Converter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var numero))
            return numero;

        if (reader.TokenType == JsonTokenType.String)
        {
            var texto = reader.GetString();
            if (int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out numero))
                return numero;
        }

        throw new JsonException(
            $"Não foi possível converter o token {reader.TokenType} em um código de localidade inteiro.");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}

/// <summary>
/// Aceita a proporção como número (0.0123) ou como texto ("0,0123" / "1,23%").
/// </summary>
public sealed class NumeroFlexivelDoubleConverter : JsonConverter<double?>
{
    public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number && reader.TryGetDouble(out var numero))
            return numero;

        if (reader.TokenType == JsonTokenType.String)
        {
            var texto = reader.GetString()?.Trim();
            if (string.IsNullOrEmpty(texto))
                return null;

            var ehPercentual = texto.EndsWith('%');
            texto = texto.TrimEnd('%').Replace("%", string.Empty).Trim();

            var estilo = texto.Contains(',') && !texto.Contains('.')
                ? NumberStyles.Number | NumberStyles.AllowThousands
                : NumberStyles.Float;

            if (double.TryParse(texto, estilo, CultureInfo.GetCultureInfo("pt-BR"), out numero)
                || double.TryParse(texto, estilo, CultureInfo.InvariantCulture, out numero))
            {
                return ehPercentual ? numero / 100d : numero;
            }
        }

        throw new JsonException(
            $"Não foi possível converter o token {reader.TokenType} em uma proporção numérica.");
    }

    public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteNumberValue(value.Value);
    }
}
