using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class Shape : SheetElement<Shape>, IPolygonSheetElement
{
    public EditablePolygon EditablePolygon { get; }
    public override IHandleSet HandleSet => EditablePolygon;

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
