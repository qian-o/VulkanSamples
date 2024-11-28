namespace Graphics.Core;

public class GraphicsException : Exception
{
    public GraphicsException(string message) : base(message)
    {
    }

    public GraphicsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
