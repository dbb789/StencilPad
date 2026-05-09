using StencilPad.Models.Operations;

namespace StencilPad.Services;

public interface IOperationService
{
    event Action<IOperation, bool>? OperationPushed;

    void Push(ICommandOperation? operation);
    void Push(IMementoOperation? operation);
}
