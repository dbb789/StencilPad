namespace StencilPad.Models;

public interface IPolygonSheetElement : ISheetElement
{
    public IEditablePolygonSet PolygonSet { get; }
}
