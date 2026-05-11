namespace StencilPad.Models.Operations;

public class AddSheetElementOperation : SheetOperation, ICommandOperation
{
    private readonly ISheetElement _sheetElement;

    public AddSheetElementOperation(Sheet sheet,
                                    ISheetElement sheetElement)
        : base(sheet)
    {
        _sheetElement = sheetElement.DeepClone();
    }

    public AddSheetElementOperation(Guid sheetId,
                                    ISheetElement sheetElement)
        : base(sheetId)
    {
        _sheetElement = sheetElement.DeepClone();
    }

    protected override void Execute(Sheet sheet)
    {
        sheet.Elements.Add(_sheetElement);
    }
    
    public override IOperation Invert()
    {
        return new RemoveSheetElementOperation(SheetId, _sheetElement);
    }
}
