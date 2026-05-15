using System.ComponentModel;
using StencilPad.Canvases.UI;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.ViewModels;

public class SheetTabViewModel : ViewModelBase, IDisposable
{
    public string Header => Sheet.Name;

    public Sheet Sheet { get; }

    private double _zoom;
    public double Zoom
    {
        get => _zoom;
        set => SetProperty(ref _zoom, value);
    }

    private bool _showGrid;
    public bool ShowGrid
    {
        get => _showGrid;
        set => SetProperty(ref _showGrid, value);
    }

    private bool _snapToGrid;
    public bool SnapToGrid
    {
        get => _snapToGrid;
        set => SetProperty(ref _snapToGrid, value);
    }

    private bool _snapToPoint;
    public bool SnapToPoint
    {
        get => _snapToPoint;
        set => SetProperty(ref _snapToPoint, value);
    }
    
    public ToolPanelViewModel ToolPanelViewModel { get; }

    public IViewport? Viewport { get; set; }
    
    public event Action<SheetCanvas>? CanvasAttached;
    public event Action? CanvasDetached;

    public SheetTabViewModel(Sheet sheet)
    {
        Sheet = sheet;
        Sheet.PropertyChanged += Sheet_PropertyChanged;
        
        _zoom = 1.0;
        _showGrid = true;
        _snapToGrid = true;
        _snapToPoint = false;
        ToolPanelViewModel = new();
    }

    public void Dispose()
    {
        Sheet.PropertyChanged -= Sheet_PropertyChanged;
    }
    
    public void AttachCanvas(SheetCanvas canvas)
    {
        CanvasAttached?.Invoke(canvas);
    }

    public void DetachCanvas()
    {
        CanvasDetached?.Invoke();
    }
    
    private void Sheet_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Sheet.Name))
        {
            OnPropertyChanged(nameof(Header));
        }
    }
}
