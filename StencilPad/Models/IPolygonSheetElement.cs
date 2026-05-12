namespace StencilPad.Models;

public interface IPolygonSheetElement : ISheetElement
{
    public IEditablePolygonSet PolygonList { get; }
}
