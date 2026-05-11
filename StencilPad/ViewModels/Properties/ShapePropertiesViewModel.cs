using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.ViewModels.Properties;

public class ShapePropertiesViewModel : ViewModelBase
{
    private readonly IEnumerable<Shape> _shapes;

    public string Title => _shapes.Count() == 1
        ? "Shape Properties"
        : $"Shape Properties ({_shapes.Count()} selected)";

    private Color _fillColor;
    public Color FillColor
    {
        get => _fillColor;
        set
        {
            _fillColor = value;

            foreach (var shape in _shapes)
            {
                shape.FillColor = value;
            }

            OnPropertyChanged();
        }
    }

    private Color _lineColor;
    public Color LineColor
    {
        get => _lineColor;
        set
        {
            _lineColor = value;

            foreach (var shape in _shapes)
            {
                shape.LineColor = value;
            }

            OnPropertyChanged();
        }
    }

    private Unit _lineWidth;
    public Unit LineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = value;

            foreach (var shape in _shapes)
            {
                shape.LineWidth = value;
            }

            OnPropertyChanged();
        }
    }

    public ShapePropertiesViewModel(IEnumerable<Shape> shapes)
    {
        _shapes = shapes;

        var first = shapes.First();

        _fillColor = first.FillColor;
        _lineColor = first.LineColor;
        _lineWidth = first.LineWidth;
    }
}
