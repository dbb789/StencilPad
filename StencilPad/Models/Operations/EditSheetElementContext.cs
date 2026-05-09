namespace StencilPad.Models.Operations;

// This is a bit heavyweight but it allows us to reliably track any operation(s)
// on sheet element(s) without having thousands of different operation types
public class EditSheetElementContext
{
    private class BulkMementoOperation : IMementoOperation
    {
        private IEnumerable<IOperation> _operations;
        
        public BulkMementoOperation(IEnumerable<IOperation> operations)
        {
            _operations = operations.ToList();
        }
        
        public void Execute(Project project)
        {
            foreach (var op in _operations)
            {
                op.Execute(project);
            }
        }
        
        public IOperation Invert()
        {
            return new BulkMementoOperation(_operations.Select(op => op.Invert()).Reverse());
        }
    }
    
    private readonly Sheet _sheet;
    private readonly List<ISheetElement> _prevElements;
    private readonly List<ISheetElement> _nextElements;

    public EditSheetElementContext(Sheet sheet,
                                   IEnumerable<ISheetElement> elements)
    {
        _sheet = sheet;
        _prevElements = elements.Select(e => e.DeepClone()).ToList();
        _nextElements = elements.ToList();
    }

    public EditSheetElementContext(Sheet sheet,
                                   ISheetElement element)
        : this(sheet, [element])
    { }
    
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

        return new BulkMementoOperation(operations);
    }
}
