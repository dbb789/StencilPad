using StencilPad.Spatial;

namespace StencilPad.Models;

public class MarkerPath : SheetElement<MarkerPath>, IPolygonSheetElement, IHandleSetSheetElement
{
    public EditablePolygon EditablePolygon { get; }
    public IHandleSet HandleSet => EditablePolygon;
    
    private Unit _spacing = Unit.FromMillimeters(4);
    public Unit Spacing
    {
        get => _spacing;
        set
        {
            if (_spacing != value)
            {
                _spacing = value;
                OnPropertyChanged();
            }
        }
    }

    private Unit _offset = Unit.FromMillimeters(2);
    public Unit Offset
    {
        get => _offset;
        set
        {
            if (_offset != value)
            {
                _offset = value;
                OnPropertyChanged();
            }
        }
    }
    
    public MarkerPath()
    {
        EditablePolygon = new EditablePolygon();
    }
    
    public MarkerPath(Polygon polygon)
    {
        EditablePolygon = new EditablePolygon(polygon);
    }
    
    private MarkerPath(EditablePolygon editablePolygon)
    {
        EditablePolygon = editablePolygon;
    }
    
    public override void Translate(Unit2D delta)
    {
        EditablePolygon.Translate(delta);
    }
    
    public override void AssignFrom(MarkerPath other)
    {
        EditablePolygon.AssignFrom(other.EditablePolygon);
        Spacing = other.Spacing;
        Offset = other.Offset;
    }

    public override MarkerPath DeepClone()
    {
        var clone = new MarkerPath(EditablePolygon.DeepClone());

        clone.Id = Id;
        
        return clone;
    }
}
