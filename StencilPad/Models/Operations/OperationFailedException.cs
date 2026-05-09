namespace StencilPad.Models.Operations;

public class OperationFailedException : Exception
{
    public OperationFailedException(string message)
        : base(message)
    { }
}
