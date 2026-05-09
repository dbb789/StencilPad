namespace StencilPad.Models;

public interface IPolygonSheetElement : ISheetElement
{
    public EditablePolygon EditablePolygon { get; }
}
