using System.Windows;
using System.Windows.Input;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.UI.Properties;

namespace StencilPad.UI;

public class WpfModelPropertiesService : IModelPropertiesService
{
    private readonly Window _owner;
    private readonly ISettings _settings;
    private readonly IResourceService _resourceService;
    private readonly IOperationService _operationService;
    private Window? _openWindow;

    public WpfModelPropertiesService(Window owner,
                                     ISettings settings,
                                     IResourceService resourceService,
                                     IOperationService operationService)
    {
        _owner = owner;
        _settings = settings;
        _resourceService = resourceService;
        _operationService = operationService;
    }

    public void CloseAll()
    {
        _openWindow?.Close();
        _openWindow = null;
    }

    public void ShowVertexCornerProperties(Sheet sheet)
    {
        _openWindow?.Close();

        var window = new VertexCornerPropertiesWindow(sheet, _settings, _operationService)
        {
            Owner = _owner
        };

        _openWindow = window;
        window.Closed += (_, _) => _openWindow = null;

        PositionAndShow(window);
    }

    public void ShowMarkerPathProperties(Sheet sheet)
    {
        _openWindow?.Close();

        var window = new MarkerPathPropertiesWindow(sheet, _settings, _resourceService, _operationService)
        {
            Owner = _owner
        };

        _openWindow = window;
        window.Closed += (_, _) => _openWindow = null;

        PositionAndShow(window);
    }

    public void ShowShapeProperties(Sheet sheet)
    {
        _openWindow?.Close();

        var window = new ShapePropertiesWindow(sheet, _settings, _resourceService, _operationService)
        {
            Owner = _owner
        };

        _openWindow = window;
        window.Closed += (_, _) => _openWindow = null;

        PositionAndShow(window);
    }
    
    public void ShowTextProperties(Sheet sheet)
    {
        _openWindow?.Close();

        var window = new TextPropertiesWindow(sheet, _settings, _operationService)
        {
            Owner = _owner
        };

        _openWindow = window;
        window.Closed += (_, _) => _openWindow = null;

        PositionAndShow(window);
    }
    
    public void ShowRulerProperties(Sheet sheet)
    {
        _openWindow?.Close();

        var window = new RulerPropertiesWindow(sheet, _settings, _operationService)
        {
            Owner = _owner
        };

        _openWindow = window;
        window.Closed += (_, _) => _openWindow = null;

        PositionAndShow(window);
    }

    public void ShowImageProperties(Sheet sheet)
    {
        _openWindow?.Close();

        var window = new ImagePropertiesWindow(sheet, _settings, _operationService)
        {
            Owner = _owner
        };

        _openWindow = window;
        window.Closed += (_, _) => _openWindow = null;

        PositionAndShow(window);
    }

    private void PositionAndShow(Window window)
    {
        window.WindowStartupLocation = WindowStartupLocation.Manual;

        var devicePoint = _owner.PointToScreen(Mouse.GetPosition(_owner));
        var source = PresentationSource.FromVisual(_owner);
        var mousePos = source != null
            ? source.CompositionTarget.TransformFromDevice.Transform(devicePoint)
            : devicePoint;

        window.Loaded += (_, _) =>
        {
            var workArea = Win32Util.GetWorkAreaForDevicePoint(devicePoint, source);

            window.Left = Math.Clamp(mousePos.X - window.ActualWidth * 0.25, workArea.Left, workArea.Right  - window.ActualWidth);
            window.Top  = Math.Clamp(mousePos.Y - window.ActualHeight * 0.25, workArea.Top,  workArea.Bottom - window.ActualHeight);
        };

        window.Show();
    }
}
