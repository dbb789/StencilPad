using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.ViewModels.Properties;

public class MarkerPathPropertiesViewModel : ElementPropertiesViewModel<MarkerPath>
{
    public string Title => "Marker Path Properties";

    private Unit _spacing;
    public Unit Spacing
    {
        get => _spacing;
        set
        {
            _spacing = value;
            
            using var context = _operationService.TryCreateEditContext(_sheet, Elements);

            foreach (var markerPath in Elements)
            {
                markerPath.Spacing = value;
            }

            OnPropertyChanged();
        }
    }

    private Unit _offset;
    public Unit Offset
    {
        get => _offset;
        set
        {
            _offset = value;
            
            using var context = _operationService.TryCreateEditContext(_sheet, Elements);

            foreach (var markerPath in Elements)
            {
                markerPath.Offset = value;
            }

            OnPropertyChanged();
        }
    }

    private Color _markerColor;
    public Color MarkerColor
    {
        get => _markerColor;
        set
        {
            _markerColor = value;
            
            using var context = _operationService.TryCreateEditContext(_sheet, Elements);

            foreach (var markerPath in Elements)
            {
                markerPath.MarkerColor = value;
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

            foreach (var markerPath in Elements)
            {
                markerPath.LineColor = value;
            }

            OnPropertyChanged();
        }
    }

    private int _markerTypeIndex;
    public int MarkerTypeIndex
    {
        get => _markerTypeIndex;
        set
        {
            _markerTypeIndex = value;
            
            using var context = _operationService.CreateEditContext(_sheet, Elements);

            foreach (var markerPath in Elements)
            {
                markerPath.MarkerType = _markerTypeIds[value];
            }

            OnPropertyChanged();
        }
    }

    public IReadOnlyList<GeometryResourceId> MarkerTypeIds => _markerTypeIds;
    
    private readonly Sheet _sheet;
    private readonly IOperationService _operationService;
    private List<GeometryResourceId> _markerTypeIds;
    private IDisposable? _dragContext;

    public MarkerPathPropertiesViewModel(Sheet sheet,
                                         IResourceService resourceService,
                                         IOperationService operationService)
        : base(sheet)
    {
        _sheet = sheet;
        _operationService = operationService;

        _markerTypeIds = new(resourceService.GetGeometryResourceIds(GeometryResourceType.Marker));
        
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
        _spacing = Mode(e => e.Spacing);
        OnPropertyChanged(nameof(Spacing));

        _offset = Mode(e => e.Offset);
        OnPropertyChanged(nameof(Offset));

        _markerColor = Mode(e => e.MarkerColor);
        OnPropertyChanged(nameof(MarkerColor));

        _lineColor = Mode(e => e.LineColor);
        OnPropertyChanged(nameof(LineColor));

        _markerTypeIndex = Mode(e => _markerTypeIds.IndexOf(e.MarkerType));
        OnPropertyChanged(nameof(MarkerTypeIndex));
    }
}
