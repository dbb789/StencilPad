using StencilPad.Spatial;

namespace StencilPad.Models.Operations;

public class BulkCommandOperation : ICommandOperation
{
    private IEnumerable<IOperation> _operations;

    public BulkCommandOperation(IEnumerable<IOperation> operations)
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
        return new BulkCommandOperation(_operations.Select(op => op.Invert()).Reverse());
    }
}
