using StencilPad.Models;
using StencilPad.Models.Operations;

namespace StencilPad.Services;

public interface IOperationService : IFlushEditContext
{
    bool HasEditContext { get; }
    
    event Action<IOperation, bool>? OperationPushed;

    IDisposable CreateEditContext(Sheet sheet,
                                  IEnumerable<ISheetElement> elements);

    IDisposable CreateEditContext(Sheet sheet, ISheetElement element);
    
    IDisposable TryCreateEditContext(Sheet sheet,
                                  IEnumerable<ISheetElement> elements);

    IDisposable TryCreateEditContext(Sheet sheet, ISheetElement element);

    void FlushEditContext();
    void DiscardEditContext();

    void Push(ICommandOperation? operation);
}
