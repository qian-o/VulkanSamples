namespace Graphics.Core;

public class ShaderCompilationException : Exception
{
    public ShaderCompilationException(string message) : base(message)
    {
    }

    public ShaderCompilationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
