using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.ViewModels.Properties;

public class MarkerPathPropertiesViewModel : ViewModelBase
{
    private readonly IReadOnlyList<MarkerPath> _markerPaths;

    public string Title => _markerPaths.Count == 1
        ? "Marker Path Properties"
        : $"Marker Path Properties ({_markerPaths.Count} selected)";

    public Unit Spacing
    {
        get => _markerPaths[0].Spacing;
        set
        {
            foreach (var markerPath in _markerPaths)
            {
                markerPath.Spacing = value;
            }

            OnPropertyChanged();
        }
    }

    public Unit Offset
    {
        get => _markerPaths[0].Offset;
        set
        {
            foreach (var markerPath in _markerPaths)
            {
                markerPath.Offset = value;
            }

            OnPropertyChanged();
        }
    }

    public MarkerPathPropertiesViewModel(IReadOnlyList<MarkerPath> markerPaths)
    {
        _markerPaths = markerPaths;
    }
}
