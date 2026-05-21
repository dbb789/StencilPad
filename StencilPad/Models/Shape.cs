using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class Shape : SheetElement<Shape>, IPolygonSheetElement
{
    public IEditablePolygonSet PolygonSet => _polygonList;

    private EditablePolygonList _polygonList;

    private Color _fillColor = Color.FromArgb(0, 255, 255, 255);
    public Color FillColor
    {
        get => _fillColor;
        set
        {
            if (_fillColor != value)
            {
                _fillColor = value;
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

    private Unit _lineWidth = Unit.FromMillimeters(0.2);
    public Unit LineWidth
    {
        get => _lineWidth;
        set
        {
            if (_lineWidth != value)
            {
                _lineWidth = value;
                OnPropertyChanged();
            }
        }
    }
    
    public Shape()
    {
        _polygonList = new();

        _polygonList.Add(new EditablePolygon());

        SetHandleSource(_polygonList.HandleSource);
    }
    
    public Shape(Polygon polygon)
    {
        _polygonList = new();
        
        var editablePolygon = new EditablePolygon();
        
        editablePolygon.AssignFrom(polygon);

        _polygonList.Add(editablePolygon);

        SetHandleSource(_polygonList.HandleSource);
    }

    public void Add(Polygon polygon)
    {
        var editablePolygon = new EditablePolygon();
        
        editablePolygon.AssignFrom(polygon);

        _polygonList.Add(editablePolygon);
    }

    public override void MirrorX(Unit centerY)
    {
        Transform = Transform with 
        { 
            Position = Transform.Position with { Y = (centerY * 2) - Transform.Position.Y },
            Angle = -Transform.Angle
        };

        foreach (var polygon in _polygonList)
        {
            polygon.MirrorX(Unit.Zero);
        }
    }

    public override void MirrorY(Unit centerX)
    {
        Transform = Transform with 
        { 
            Position = Transform.Position with { X = (centerX * 2) - Transform.Position.X },
            Angle = -Transform.Angle
        };

        foreach (var polygon in _polygonList)
        {
            polygon.MirrorY(Unit.Zero);
        }
    }

    public override void Translate(Unit2D delta)
    {
        Transform = Transform with { Position = Transform.Position + delta };
    }

    public override void AssignFrom(Shape other)
    {
        _polygonList.AssignFrom(other._polygonList);

        Transform = other.Transform;
        FillColor = other.FillColor;
        LineColor = other.LineColor;
        LineWidth = other.LineWidth;
    }
    
    public override Shape DeepClone()
    {
        var clone = new Shape();

        clone.Id = Id;
        clone.AssignFrom(this);
        
        return clone;
    }
}
