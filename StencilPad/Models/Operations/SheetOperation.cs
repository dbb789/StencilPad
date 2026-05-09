namespace StencilPad.Models.Operations;

public abstract class SheetOperation
{
    protected readonly Guid SheetId;

    public SheetOperation(Guid sheetId)
    {
        SheetId = sheetId;
    }

    public SheetOperation(Sheet sheet)
    {
        SheetId = sheet.Id;
    }

    public void Execute(Project project)
    {
        if (!project.TryGetSheet(SheetId, out var sheet))
        {
            throw new OperationFailedException($"Sheet with id {SheetId} not found");
        }

        Execute(sheet);
    }

    protected abstract void Execute(Sheet sheet);

    public abstract IOperation Invert();
}
