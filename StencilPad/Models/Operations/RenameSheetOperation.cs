namespace StencilPad.Models.Operations;

public class RenameSheetOperation : ICommandOperation
{
    private readonly Guid _sheetId;
    private readonly string _previousName;
    private readonly string _nextName;
    
    public RenameSheetOperation(Sheet sheet,
                                string nextName)
    {
        _sheetId = sheet.Id;
        _previousName = sheet.Name;
        _nextName = nextName;
    }

    private RenameSheetOperation(Guid sheetId,
                                 string previousName,
                                 string nextName)
    {
        _sheetId = sheetId;
        _previousName = previousName;
        _nextName = nextName;
    }

    public void Execute(Project project, out Sheet? targetSheet)
    {
        if (!project.Sheets.TryGetValue(_sheetId, out var sheet))
        {
            throw new OperationFailedException($"Sheet with id {_sheetId} not found");
        }

        sheet.Name = _nextName;
        targetSheet = sheet;
    }
    
    public IOperation Invert()
    {
        return new RenameSheetOperation(_sheetId, _nextName, _previousName);
    }
}
