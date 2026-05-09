using StencilPad.Models.Operations;

namespace StencilPad.Services;

public class OperationService : IOperationService
{
    public event Action<IOperation, bool>? OperationPushed;

    public void Push(ICommandOperation? operation)
    {
        if (operation is null)
        {
            return;
        }
        
        OperationPushed?.Invoke(operation, true);
    }

    public void Push(IMementoOperation? operation)
    {
        if (operation is null)
        {
            return;
        }
        
        OperationPushed?.Invoke(operation, false);
    }
}
