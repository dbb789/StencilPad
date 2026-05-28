using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Services;
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

    private int _startCapIndex;
    public int StartCapIndex
    {
        get => _startCapIndex;
        set
        {
            _startCapIndex = value;

            foreach (var shape in _shapes)
            {
                shape.StartCap = _capIds[value];
            }

            OnPropertyChanged();
        }
    }

    private int _endCapIndex;
    public int EndCapIndex
    {
        get => _endCapIndex;
        set
        {
            _endCapIndex = value;

            foreach (var shape in _shapes)
            {
                shape.EndCap = _capIds[value];
            }

            OnPropertyChanged();
        }
    }

    private int _lineStyleIndex;
    public int LineStyleIndex
    {
        get => _lineStyleIndex;
        set
        {
            _lineStyleIndex = value;

            foreach (var shape in _shapes)
            {
                shape.LineStyle = _lineStyles[value];
            }

            OnPropertyChanged();
        }
    }
    
    public IReadOnlyList<GeometryResourceId> CapIds => _capIds;
    public IReadOnlyList<LineStyleResourceId> LineStyleIds => _lineStyles;
    
    private List<GeometryResourceId> _capIds;
    private List<LineStyleResourceId> _lineStyles;
    
    public ShapePropertiesViewModel(IResourceService resourceService,
                                    IEnumerable<Shape> shapes)
    {
        _capIds = [ GeometryResourceId.None ];
        _capIds.AddRange(resourceService.GetGeometryResourceIds(GeometryResourceType.Cap));

        _lineStyles = [];
        _lineStyles.AddRange(resourceService.GetLineStyleResourceIds());
        
        _shapes = shapes;

        var first = shapes.First();

        _fillColor = first.FillColor;
        _lineColor = first.LineColor;
        _lineWidth = first.LineWidth;
    }
}
