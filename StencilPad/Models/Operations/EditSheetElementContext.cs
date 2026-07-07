using System.Diagnostics;

namespace StencilPad.Models.Operations;

// This is a bit heavyweight but it allows us to reliably track any operation(s)
// on sheet element(s) without having thousands of different operation types
public class EditSheetElementContext : IEditContext
{
    private class BulkMementoOperation : IMementoOperation
    {
        private Sheet _targetSheet;
        private IEnumerable<IOperation> _operations;
        
        public BulkMementoOperation(Sheet targetSheet, IEnumerable<IOperation> operations)
        {
            _targetSheet = targetSheet;
            _operations = operations.ToList();
        }
        
        public void Execute(Project project, out Sheet? targetSheet)
        {
            foreach (var op in _operations)
            {
                op.Execute(project, out var sheet);
            }
            
            targetSheet = _targetSheet;
        }
        
        public IOperation Invert()
        {
            return new BulkMementoOperation(_targetSheet,
                                            _operations.Select(op => op.Invert()).Reverse());
        }
    }
    
    private readonly Sheet _sheet;
    private readonly List<ISheetElement> _prevElements;
    private readonly List<ISheetElement> _nextElements;
    private IFlushEditContext? _target;

    public EditSheetElementContext(Sheet sheet,
                                   IEnumerable<ISheetElement> elements,
                                   IFlushEditContext target)
    {
        _sheet = sheet;
        _prevElements = elements.Select(e => e.DeepClone()).ToList();
        _nextElements = elements.ToList();
        _target = target;
    }

    public EditSheetElementContext(Sheet sheet,
                                   ISheetElement element,
                                   IFlushEditContext target)
        : this(sheet, [element], target)
    { }

    public void Dispose()
    {
        if (_target is null)
        {
            Debug.WriteLine("EditSheetElementContext disposed multiple times");
            return;
        }
        
        _target.Flush(this, FlushOperation());
        _target = null;
    }

    public void Discard()
    {
        _target = null;
    }
    
    public IMementoOperation? FlushOperation()
    {
        if (_prevElements.Count == 0)
        {
            return null;
        }
        
        var operations = new List<IOperation>(_prevElements.Count);

        for (int i = 0; i < _prevElements.Count; ++i)
        {
            operations.Add(new EditSheetElementOperation(_sheet,
                                                         _prevElements[i],
                                                         _nextElements[i]));
        }

        return new BulkMementoOperation(_sheet, operations);
    }
}
