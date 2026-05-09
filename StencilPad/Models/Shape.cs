using StencilPad.Spatial;

namespace StencilPad.Models;

public class Shape : SheetElement<Shape>, IPolygonSheetElement, IHandleSetSheetElement
{
    public EditablePolygon EditablePolygon { get; }
    public IHandleSet HandleSet => EditablePolygon;

    public Shape()
    {
        EditablePolygon = new EditablePolygon();
    }
    
    public Shape(Polygon polygon)
    {
        EditablePolygon = new EditablePolygon(polygon);
    }
    
    private Shape(EditablePolygon editablePolygon)
    {
        EditablePolygon = editablePolygon;
    }

    public override void Translate(Unit2D delta)
    {
        EditablePolygon.Translate(delta);
    }

    public override void AssignFrom(Shape other)
    {
        EditablePolygon.AssignFrom(other.EditablePolygon);
    }
    
    public override Shape DeepClone()
    {
        var clone = new Shape(EditablePolygon.DeepClone());

        clone.Id = Id;
        
        return clone;
    }
}
