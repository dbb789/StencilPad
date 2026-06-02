using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class MarkerPath : SheetElement<MarkerPath>, IPolygonSheetElement
{
    public IEditablePolygonSet PolygonSet => _singlePolygon;
    public MarkerPathPointList PointList => _pointList;

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
                
                UpdateGeometry();
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
                
                UpdateGeometry();
                OnPropertyChanged();
            }
        }
    }
    
    private bool _balanced = true;
    public bool Balanced
    {
        get => _balanced;
        set
        {
            if (_balanced != value)
            {
                _balanced = value;
                UpdateGeometry();
                OnPropertyChanged();
            }
        }
    }

    public GeometryResourceId _markerType = GeometryResourceId.DefaultMarker;
    public GeometryResourceId MarkerType
    {
        get => _markerType;
        set
        {
            if (_markerType != value)
            {
                _markerType = value;
                OnPropertyChanged();
            }
        }
    }
    
    private Color _markerColor = Color.FromArgb(255, 0, 0, 0);
    public Color MarkerColor
    {
        get => _markerColor;
        set
        {
            if (_markerColor != value)
            {
                _markerColor = value;
                OnPropertyChanged();
            }
        }
    }

    private Color _lineColor = Color.FromArgb(255, 0, 0, 0);
    public Color LineColor
    {
        get => _lineColor;
        set
        {
            if (_lineColor != value)
            {
                _lineColor = value;
                OnPropertyChanged();
            }
        }
    }

    public bool HasBalancePoint => _pointList.Balanced;

    private MarkerPathPointList _pointList;

    public MarkerPath()
    {
        _singlePolygon = new();
        _singlePolygon.Polygon.GeometryChanged += _ => UpdateGeometry();
        _pointList = new();
        
        SetHandleSource(_singlePolygon.HandleSource);
    }
    
    public MarkerPath(Polygon polygon)
    {
        _singlePolygon = new(polygon);
        _singlePolygon.Polygon.GeometryChanged += _ => UpdateGeometry();
        _pointList = new();

        SetHandleSource(_singlePolygon.HandleSource);
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

    public override void NormalizePosition()
    {
        var midpoint = Polygon.CalculateMidpoint();
        Polygon.Translate(-midpoint);
        Transform = Transform with { Position = Transform.Position + Transform.Rotate(midpoint) };
    }

    public override UnitBounds GetBounds(UnitTransform transform)
    {
        return Polygon.CalculateBounds(transform);
    }
    
    public override void SetBounds(UnitBounds newBounds, UnitTransform transform)
    {
        var oldBounds = Polygon.CalculateBounds(transform);
        Polygon.SetBounds(oldBounds, newBounds, transform);
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

    private void UpdateGeometry()
    {
        _pointList.GeneratePoints(Polygon, Spacing, Offset, Balanced);
        
        FireGeometryChanged();
    }
}
