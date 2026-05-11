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
    
    public void Execute(Project project)
    {
        foreach (var op in _operations)
        {
            op.Execute(project);
        }
    }

    public IOperation Invert()
    {
        var inverted = new BulkCommandOperation();

        inverted._operations.AddRange(_operations.Select(op => op.Invert()).Reverse());

        return inverted;
    }
}
