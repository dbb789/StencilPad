using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.ViewModels.Properties;

public class MarkerPathPropertiesViewModel : ViewModelBase
{
    private readonly IEnumerable<MarkerPath> _markerPaths;

    public string Title => _markerPaths.Count() == 1
        ? "Marker Path Properties"
        : $"Marker Path Properties ({_markerPaths.Count()} selected)";

    private Unit _spacing;
    public Unit Spacing
    {
        get => _spacing;
        set
        {
            _spacing = value;
            
            foreach (var markerPath in _markerPaths)
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
            
            foreach (var markerPath in _markerPaths)
            {
                markerPath.Offset = value;
            }

            OnPropertyChanged();
        }
    }

    public MarkerPathPropertiesViewModel(IEnumerable<MarkerPath> markerPaths)
    {   
        _markerPaths = markerPaths;

        var first = markerPaths.First();

        _spacing = first.Spacing;
        _offset = first.Offset;
    }
}
