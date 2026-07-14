namespace StencilPad.Models.Operations;

public class ReorderSheetOperation : ICommandOperation
{
    private readonly int _fromIndex;
    private readonly int _toIndex;
    
    public ReorderSheetOperation(int fromIndex,
                                 int toIndex)
    {
        _fromIndex = fromIndex;
        _toIndex = toIndex;
    }
    
    public void Execute(Project project, out Sheet? targetSheet)
    {
        project.Sheets.Move(_fromIndex, _toIndex);
        targetSheet = project.Sheets[_toIndex];
    }
    
    public IOperation Invert()
    {
        return new ReorderSheetOperation(_toIndex, _fromIndex);
    }
}
