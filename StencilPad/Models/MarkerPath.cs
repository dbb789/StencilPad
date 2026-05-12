using StencilPad.Spatial;

namespace StencilPad.Models;

public class MarkerPath : SheetElement<MarkerPath>, IPolygonSheetElement
{
    public IEditablePolygonSet PolygonList => _singlePolygon;
    public override IHandleSet HandleSet => _singlePolygon.HandleSet;

    public EditablePolygon Polygon => _singlePolygon.Polygon;
    private SingleEditablePolygon _singlePolygon;
 
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
        _singlePolygon = new();
    }
    
    public MarkerPath(Polygon polygon)
    {
        _singlePolygon = new(polygon);
    }
    
    public override void MirrorX(Unit centerY)
    {
        Polygon.MirrorX(centerY);
    }

    public override void MirrorY(Unit centerX)
    {
        Polygon.MirrorY(centerX);
    }

    public override void Translate(Unit2D delta)
    {
        Polygon.Translate(delta);
    }

    public override void AssignFrom(MarkerPath other)
    {
        _singlePolygon.AssignFrom(other._singlePolygon);
        
        Spacing = other.Spacing;
        Offset = other.Offset;
    }

    public override MarkerPath DeepClone()
    {
        var clone = new MarkerPath();

        clone.Id = Id;
        clone.AssignFrom(this);
        
        return clone;
    }
}
