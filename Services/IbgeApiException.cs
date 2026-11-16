namespace DemografiaLocalApp.Services;

/// <summary>
/// Exceção lançada quando a comunicação com as APIs do IBGE falha
/// (rede indisponível, timeout, erro HTTP ou JSON inesperado).
/// A UI deve capturá-la e exibir <see cref="Exception.Message"/> ao usuário.
/// </summary>
public sealed class IbgeApiException : Exception
{
    public IbgeApiException(string message) : base(message)
    {
    }

    public IbgeApiException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Código HTTP retornado pelo IBGE, quando houver resposta.</summary>
    public int? StatusCode { get; init; }
}
