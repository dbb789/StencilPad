namespace StencilPad.Models.Operations;

public class EditSheetElementOperation : SheetOperation, ICommandOperation
{
    private readonly ISheetElement _prevElement;
    private readonly ISheetElement _nextElement;

    public EditSheetElementOperation(Sheet sheet,
                                     ISheetElement prevElement,
                                     ISheetElement nextElement)
        : base(sheet)
    {
        _prevElement = prevElement.DeepClone();
        _nextElement = nextElement.DeepClone();
    }

    public EditSheetElementOperation(Guid sheetId,
                                     ISheetElement prevElement,
                                     ISheetElement nextElement)
        : base(sheetId)
    {
        _prevElement = prevElement.DeepClone();
        _nextElement = nextElement.DeepClone();
    }

    protected override void Execute(Sheet sheet)
    {
        sheet.AssignElement(_nextElement);
    }

    public override IOperation Invert()
    {
        return new EditSheetElementOperation(SheetId,
                                             _nextElement,
                                             _prevElement);
    }
}
