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

            foreach (var markerPath in Elements)
            {
                markerPath.LineColor = value;
            }

            OnPropertyChanged();
        }
    }

    public MarkerPathPropertiesViewModel(IResourceService resourceService,
                                         Sheet sheet)
        : base(sheet)
    {
        OnElementsChanged();
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
    }
}
