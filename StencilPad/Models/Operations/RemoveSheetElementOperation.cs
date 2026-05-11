namespace StencilPad.Models.Operations;

public class RemoveSheetElementOperation : SheetOperation, ICommandOperation
{
    private readonly ISheetElement _sheetElement;
    
    public RemoveSheetElementOperation(Sheet sheet,
                                       ISheetElement sheetElement)
        : base(sheet)
    {
        _sheetElement = sheetElement.DeepClone();
    }

    public RemoveSheetElementOperation(Guid sheetId,
                                       ISheetElement sheetElement)
        : base(sheetId)
    {
        _sheetElement = sheetElement.DeepClone();
    }

    protected override void Execute(Sheet sheet)
    {
        sheet.Elements.Remove(_sheetElement);
    }
    
    public override IOperation Invert()
    {
        return new AddSheetElementOperation(SheetId, _sheetElement);
    }
}
