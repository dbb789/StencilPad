namespace StencilPad.Models.Operations;

public class BulkCommandOperation : ICommandOperation
{
    private List<IOperation> _operations;

    public BulkCommandOperation(IEnumerable<ICommandOperation> operations)
    {
        _operations = new(operations);
    }

    public BulkCommandOperation()
    {
        _operations = new(2);
    }

    public void Add(ICommandOperation operation)
    {
        _operations.Add(operation);
    }
    
    public void Execute(Project project, out Sheet? targetSheet)
    {
        targetSheet = null;
        
        foreach (var op in _operations)
        {
            op.Execute(project, out var sheet);

            // Target sheet is the last sheet that was modified by the operations.
            if (sheet is not null)
            {
                targetSheet = sheet;
            }
        }
    }

    public IOperation Invert()
    {
        var inverted = new BulkCommandOperation();

        inverted._operations.AddRange(_operations.Select(op => op.Invert()).Reverse());

        return inverted;
    }
}
