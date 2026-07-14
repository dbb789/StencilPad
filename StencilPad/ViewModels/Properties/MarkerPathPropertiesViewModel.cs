using System.Windows.Media;
using StencilPad.Common;
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
            SetElementProperty(e => e.Spacing = value);
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
            SetElementProperty(e => e.Offset = value);
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
            SetElementProperty(e => e.MarkerColor = value);
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
            SetElementProperty(e => e.LineWidth = value);
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
            SetElementProperty(e => e.LineColor = value);
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
            SetElementProperty(e => e.MarkerType = _markerTypeIds[value]);
            OnPropertyChanged();
        }
    }

    private bool _balanced;
    public bool Balanced
    {
        get => _balanced;
        set
        {
            _balanced = value;
            SetElementProperty(e => e.Balanced = value);
            OnPropertyChanged();
        }
    }
    
    public IReadOnlyList<GeometryResourceId> MarkerTypeIds => _markerTypeIds;

    private List<GeometryResourceId> _markerTypeIds;
    private IDisposable? _dragContext;

    public MarkerPathPropertiesViewModel(Sheet sheet,
                                         ISettings settings,
                                         IResourceService resourceService,
                                         IOperationService operationService)
        : base(sheet, operationService, settings)
    {
        _markerTypeIds = new(resourceService.GetGeometryResourceIds(GeometryResourceType.Marker));
        
        OnElementsChanged();
    }
    
    public void DragBegin()
    {
        _dragContext = OperationService.CreateEditContext(Sheet, Elements);
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

        _lineWidth = Mode(e => e.LineWidth);
        OnPropertyChanged(nameof(LineWidth));
        
        _markerTypeIndex = Mode(e => _markerTypeIds.IndexOf(e.MarkerType));
        OnPropertyChanged(nameof(MarkerTypeIndex));

        _balanced = Mode(e => e.Balanced);
        OnPropertyChanged(nameof(Balanced));
    }
}
