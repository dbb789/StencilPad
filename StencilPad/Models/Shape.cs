using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class Shape : SheetElement<Shape>, IPolygonSheetElement
{
    public IEditablePolygonSet PolygonSet => _polygonList;
    public override IHandleSet HandleSet => _polygonList.HandleSet;

    private EditablePolygonList _polygonList;

    public Unit2D _position = Unit2D.Zero;
    public Unit2D Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                _polygonList.Position = value;                
                OnPropertyChanged();
            }
        }
    }
    
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
        _position = Unit2D.Zero;

        _polygonList.Add(new EditablePolygon());
    }
    
    public Shape(Polygon polygon)
    {
        _polygonList = new();
        _position = Unit2D.Zero;
        
        var editablePolygon = new EditablePolygon();
        
        editablePolygon.AssignFrom(polygon);

        _polygonList.Add(editablePolygon);
    }

    public void Add(Polygon polygon)
    {
        var editablePolygon = new EditablePolygon();
        
        editablePolygon.AssignFrom(polygon);

        _polygonList.Add(editablePolygon);
    }

    public override void MirrorX(Unit centerY)
    {
        foreach (var polygon in _polygonList)
        {
            polygon.MirrorX(centerY - Position.Y);
        }
    }

    public override void MirrorY(Unit centerX)
    {
        foreach (var polygon in _polygonList)
        {
            polygon.MirrorY(centerX - Position.X);
        }
    }

    public override void Translate(Unit2D delta)
    {
        Position += delta;
    }

    public override void AssignFrom(Shape other)
    {
        _polygonList.AssignFrom(other._polygonList);

        Position = other.Position;
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
