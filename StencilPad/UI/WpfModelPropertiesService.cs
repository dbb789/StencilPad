using System.Windows;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.UI.Properties;

namespace StencilPad.UI;

public class WpfModelPropertiesService : IModelPropertiesService
{
    private readonly Window _owner;
    private readonly IOperationService _operationService;
    private Window? _openWindow;

    public WpfModelPropertiesService(Window owner, IOperationService operationService)
    {
        _owner = owner;
        _operationService = operationService;
    }

    public void CloseAll()
    {
        _openWindow?.Close();
        _openWindow = null;
    }

    public void ShowVertexCornerProperties(Sheet sheet, IReadOnlyList<VertexCornerTarget> targets)
    {
        _openWindow?.Close();

        var window = new VertexCornerPropertiesWindow(sheet, targets, _operationService)
        {
            Owner = _owner
        };

        _openWindow = window;

        window.Closed += (_, _) => _openWindow = null;

        window.Show();
    }

    public void ShowMarkerPathProperties(IReadOnlyList<MarkerPath> MarkerPaths)
    {
        _openWindow?.Close();

        var window = new MarkerPathPropertiesWindow(MarkerPaths)
        {
            Owner = _owner
        };

        _openWindow = window;

        window.Closed += (_, _) => _openWindow = null;

        window.Show();
    }
}
