using StencilPad.Models;
using StencilPad.Models.Operations;

namespace StencilPad.Services;

public interface IOperationService : IFlushEditContext
{
    bool HasEditContext { get; }
    
    event Action<IOperation, bool>? OperationPushed;

    IDisposable CreateEditContext(Sheet sheet,
                                  IEnumerable<ISheetElement> elements);
    void FlushEditContext();
    void DiscardEditContext();

    void Push(ICommandOperation? operation);
}
