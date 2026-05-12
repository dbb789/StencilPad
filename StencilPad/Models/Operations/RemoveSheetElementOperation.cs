namespace StencilPad.Models.Operations;

public class RemoveSheetElementOperation : SheetOperation, ICommandOperation
{
    private readonly ISheetElement _sheetElement;
    private int _index;
    
    public RemoveSheetElementOperation(Sheet sheet,
                                       ISheetElement sheetElement)
        : base(sheet)
    {
        _sheetElement = sheetElement.DeepClone();
        _index = -1;
    }

    public RemoveSheetElementOperation(Guid sheetId,
                                       ISheetElement sheetElement)
        : base(sheetId)
    {
        _sheetElement = sheetElement.DeepClone();
        _index = -1;
    }

    protected override void Execute(Sheet sheet)
    {
        _index = sheet.Elements.IndexOf(_sheetElement);
        sheet.Elements.Remove(_sheetElement);
    }
    
    public override IOperation Invert()
    {
        return new AddSheetElementOperation(SheetId, _sheetElement, _index);
    }
}
