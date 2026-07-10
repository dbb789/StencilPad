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
        // NOTE: Recording the index where we actually removed the element
        // should be fine here, since we're an ICommandOperation that gets
        // executed immediately, so _index should be populated before any call
        // to Invert().
        
        _index = sheet.Elements.IndexOf(_sheetElement);
        sheet.Elements.Remove(_sheetElement.Id);
    }
    
    public override IOperation Invert()
    {
        return new AddSheetElementOperation(SheetId, _sheetElement, _index);
    }
}
