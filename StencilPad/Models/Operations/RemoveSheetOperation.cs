namespace StencilPad.Models.Operations;

public class RemoveSheetOperation : ICommandOperation
{
    private readonly Sheet _sheet;
    private int _index;
    
    public RemoveSheetOperation(Sheet sheet)
    {
        _sheet = sheet.DeepClone();
        _index = -1;
    }
    
    public void Execute(Project project, out Sheet? targetSheet)
    {
        targetSheet = null;

        // NOTE: Recording the index where we actually removed the element
        // should be fine here, since we're an ICommandOperation that gets
        // executed immediately, so _index should be populated before any call
        // to Invert().
        
        _index = project.Sheets.IndexOf(_sheet);
        project.Sheets.Remove(_sheet.Id);
    }
    
    public IOperation Invert()
    {
        return new AddSheetOperation(_sheet, _index);
    }
}
