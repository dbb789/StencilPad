namespace StencilPad.Models.Operations;

public interface IFlushEditContext
{
    void Flush(IEditContext context, IMementoOperation? operation);
}
