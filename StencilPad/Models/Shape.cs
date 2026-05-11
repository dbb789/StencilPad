using StencilPad.Spatial;

namespace StencilPad.Models;

public class Shape : SheetElement<Shape>, IPolygonSheetElement
{
    public EditablePolygon EditablePolygon { get; }
    public override IHandleSet HandleSet => EditablePolygon;

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

    public override void MirrorX(Unit centerY)
    {
        EditablePolygon.MirrorX(centerY);
    }

    public override void MirrorY(Unit centerX)
    {
        EditablePolygon.MirrorY(centerX);
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
        var clone = new Shape();

        clone.Id = Id;
        clone.AssignFrom(this);
        
        return clone;
    }
}
