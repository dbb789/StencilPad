using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.ViewModels.Properties;

public class ShapePropertiesViewModel : ElementPropertiesViewModel<Shape>
{
    public string Title => "Shape Properties";

    private Color _fillColor;
    public Color FillColor
    {
        get => _fillColor;
        set
        {
            _fillColor = value;

            // Note the use of TryCreateEditContext here - this is because we
            // may or may not be in the middle of a drag operation, and we don't
            // want to create an edit context if we are. Same goes for LineColor
            // below.
            using var context = _operationService.TryCreateEditContext(_sheet, Elements);

            foreach (var shape in Elements)
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
            
            using var context = _operationService.TryCreateEditContext(_sheet, Elements);

            foreach (var shape in Elements)
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
            
            using var context = _operationService.CreateEditContext(_sheet, Elements);

            foreach (var shape in Elements)
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
            
            using var context = _operationService.CreateEditContext(_sheet, Elements);

            foreach (var shape in Elements)
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

            using var context = _operationService.CreateEditContext(_sheet, Elements);

            foreach (var shape in Elements)
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

            using var context = _operationService.CreateEditContext(_sheet, Elements);

            foreach (var shape in Elements)
            {
                shape.LineStyle = _lineStyles[value];
            }

            OnPropertyChanged();
        }
    }
    
    public IReadOnlyList<GeometryResourceId> CapIds => _capIds;
    public IReadOnlyList<LineStyleResourceId> LineStyleIds => _lineStyles;

    private readonly Sheet _sheet;
    private readonly IOperationService _operationService;
    private List<GeometryResourceId> _capIds;
    private List<LineStyleResourceId> _lineStyles;
    private IDisposable? _dragContext;
    
    public ShapePropertiesViewModel(Sheet sheet,
                                    IResourceService resourceService,
                                    IOperationService operationService)
        : base(sheet)
    {
        _sheet = sheet;
        _operationService = operationService;

        _capIds = [ GeometryResourceId.None ];
        _capIds.AddRange(resourceService.GetGeometryResourceIds(GeometryResourceType.Cap));

        _lineStyles = [];
        _lineStyles.AddRange(resourceService.GetLineStyleResourceIds());

        OnElementsChanged();
    }

    public void DragBegin()
    {
        _dragContext = _operationService.CreateEditContext(_sheet, Elements);
    }

    public void DragEnd()
    {
        _dragContext?.Dispose();
    }
    
    protected override void OnElementsChanged()
    {
        _fillColor = Mode(shape => shape.FillColor);
        OnPropertyChanged(nameof(FillColor));

        _lineColor = Mode(shape => shape.LineColor);
        OnPropertyChanged(nameof(LineColor));

        _lineWidth = Mode(shape => shape.LineWidth);
        OnPropertyChanged(nameof(LineWidth));

        _startCapIndex = Mode(shape => _capIds.IndexOf(shape.StartCap));
        OnPropertyChanged(nameof(StartCapIndex));

        _endCapIndex = Mode(shape => _capIds.IndexOf(shape.EndCap));
        OnPropertyChanged(nameof(EndCapIndex));

        _lineStyleIndex = Mode(shape => _lineStyles.IndexOf(shape.LineStyle));
        OnPropertyChanged(nameof(LineStyleIndex));
    }
}
