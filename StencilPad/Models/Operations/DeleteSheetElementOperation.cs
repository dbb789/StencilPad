namespace StencilPad.Models.Operations;

public class DeleteSheetElementOperation : SheetOperation, ICommandOperation
{
    private readonly ISheetElement _sheetElement;
    
    public DeleteSheetElementOperation(Sheet sheet,
                                       ISheetElement sheetElement)
        : base(sheet)
    {
        _sheetElement = sheetElement.DeepClone();
    }

    public DeleteSheetElementOperation(Guid sheetId,
                                       ISheetElement sheetElement)
        : base(sheetId)
    {
        _sheetElement = sheetElement.DeepClone();
    }

    protected override void Execute(Sheet sheet)
    {
        sheet.RemoveElement(_sheetElement.Id);
    }
    
    public override IOperation Invert()
    {
        return new AddSheetElementOperation(SheetId, _sheetElement);
    }
}
