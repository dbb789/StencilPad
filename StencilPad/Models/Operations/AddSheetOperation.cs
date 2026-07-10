namespace StencilPad.Models.Operations;

public class AddSheetOperation : ICommandOperation
{
    private readonly Sheet _sheet;
    private readonly int _index = -1;
    
    public AddSheetOperation(Sheet sheet,
                             int index = -1)
    {
        _sheet = sheet.DeepClone();
        _index = index;
    }

    public void Execute(Project project, out Sheet? targetSheet)
    {
        var clone = _sheet.DeepClone();
        
        if (_index < 0)
        {
            project.Sheets.Add(clone.Id, clone);
        }
        else
        {
            project.Sheets.Insert(_index, clone.Id, clone);
        }

        targetSheet = _sheet;
    }
    
    public IOperation Invert()
    {
        return new RemoveSheetOperation(_sheet);
    }
}
