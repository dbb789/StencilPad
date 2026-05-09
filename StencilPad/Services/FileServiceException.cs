namespace StencilPad.Services;

public class FileServiceException : Exception
{
    public FileServiceException(string message)
        : base(message)
    { }

    public FileServiceException(string message, Exception inner)
        : base(message, inner)
    { }
}
