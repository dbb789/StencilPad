namespace StencilPad.Models.Operations;

public class AddSheetElementOperation : SheetOperation, ICommandOperation
{
    private readonly ISheetElement _sheetElement;
    private readonly int _index = -1;
    
    public AddSheetElementOperation(Sheet sheet,
                                    ISheetElement sheetElement,
                                    int index = -1)
        : base(sheet)
    {
        _sheetElement = sheetElement.DeepClone();
        _index = index;
    }

    public AddSheetElementOperation(Guid sheetId,
                                    ISheetElement sheetElement,
                                    int index = -1)
        : base(sheetId)
    {
        _sheetElement = sheetElement.DeepClone();
        _index = index;
    }

    protected override void Execute(Sheet sheet)
    {
        if (_index == -1)
        {
            sheet.Elements.Add(_sheetElement);
        }
        else
        {
            sheet.Elements.Insert(_index, _sheetElement);
        }
    }
    
    public override IOperation Invert()
    {
        return new RemoveSheetElementOperation(SheetId, _sheetElement);
    }
}
