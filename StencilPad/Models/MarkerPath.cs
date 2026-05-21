using StencilPad.Spatial;

namespace StencilPad.Models;

public class MarkerPath : SheetElement<MarkerPath>, IPolygonSheetElement
{
    public IEditablePolygonSet PolygonSet => _singlePolygon;
    public override IHandleSource HandleSource => _singlePolygon.HandleSource;

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
        Transform = Transform with 
        { 
            Position = Transform.Position with { Y = (centerY * 2) - Transform.Position.Y },
            Angle = -Transform.Angle
        };

        Polygon.MirrorX(Unit.Zero);
    }

    public override void MirrorY(Unit centerX)
    {
        Transform = Transform with 
        { 
            Position = Transform.Position with { X = (centerX * 2) - Transform.Position.X },
            Angle = -Transform.Angle
        };

        Polygon.MirrorY(Unit.Zero);
    }

    public override void Translate(Unit2D delta)
    {
        Transform = Transform with { Position = Transform.Position + delta };
    }

    public override void AssignFrom(MarkerPath other)
    {
        _singlePolygon.AssignFrom(other._singlePolygon);
        Transform = other.Transform;
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
