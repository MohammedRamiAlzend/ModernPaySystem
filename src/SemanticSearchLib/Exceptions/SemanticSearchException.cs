namespace SemanticSearchLib.Exceptions;

public class SemanticSearchException : Exception
{
    public SemanticSearchException(string message) : base(message)
    {
    }

    public SemanticSearchException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
