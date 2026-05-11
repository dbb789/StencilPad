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
        if (!sheet.RemoveElement(_sheetElement.Id))
        {
            throw new InvalidOperationException($"Failed to remove element with id {_sheetElement.Id} from sheet with id {sheet.Id}");
        }
    }
    
    public override IOperation Invert()
    {
        return new AddSheetElementOperation(SheetId, _sheetElement);
    }
}
