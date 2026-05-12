namespace StencilPad.Models.Operations;

public class ReorderSheetElementOperation : SheetOperation, ICommandOperation
{
    private readonly int _fromIndex;
    private readonly int _toIndex;
    
    public ReorderSheetElementOperation(Sheet sheet,
                                        int fromIndex,
                                        int toIndex)
        : base(sheet)
    {
        _fromIndex = fromIndex;
        _toIndex = toIndex;
    }

    public ReorderSheetElementOperation(Guid sheetId,
                                        int fromIndex,
                                        int toIndex)
        : base(sheetId)
    {
        _fromIndex = fromIndex;
        _toIndex = toIndex;
    }

    protected override void Execute(Sheet sheet)
    {
        sheet.Elements.Move(_fromIndex, _toIndex);
    }
    
    public override IOperation Invert()
    {
        return new ReorderSheetElementOperation(SheetId, _toIndex, _fromIndex);
    }
}
