using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StencilPad.Canvases.UI;
using StencilPad.ViewModels;

namespace StencilPad.UI;

public partial class SheetTab : UserControl
{
    private const double ZoomStep = 1.1;
    private const double ZoomMin = 0.1;
    private const double ZoomMax = 3.0;

    private bool _showGrid = true;
    public bool ShowGrid
    {
        get => _showGrid;
        set => _showGrid = value;
    }

    private double _zoom = 1.0;
    public double Zoom
    {
        get => _zoom;
        set => _zoom = value;
    }

    private bool _snapToGrid = true;
    public bool SnapToGrid
    {
        get => _snapToGrid;
        set => _snapToGrid = value;
    }
    
    private Point _lastMousePosition;
    private double _lastHorizontalOffset;
    private double _lastVerticalOffset;
    
    public SheetTab()
    {
        InitializeComponent();

        SheetCanvas.CanvasReady += SheetCanvasReady;
        
        Scroll.PreviewMouseWheel += OnPreviewMouseWheel;
        Scroll.PreviewMouseDown += OnPreviewMouseDown;
        Scroll.PreviewMouseMove += OnPreviewMouseMove;
        Scroll.PreviewMouseUp += OnPreviewMouseUp;
    }

    private void SheetCanvasReady()
    {
        (DataContext as SheetTabViewModel)?.AttachCanvas(SheetCanvas);
        DataContextChanged += (s, e) =>
        {
            if (e.OldValue is SheetTabViewModel oldVm)
            {
                oldVm.DetachCanvas();
            }
            
            if (e.NewValue is SheetTabViewModel newVm)
            {
                newVm.AttachCanvas(SheetCanvas);
            }
        };
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle)
        {
            _lastMousePosition = e.GetPosition(this);
            _lastHorizontalOffset = Scroll.HorizontalOffset;
            _lastVerticalOffset = Scroll.VerticalOffset;
            Scroll.CaptureMouse();
            e.Handled = true;
        }
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (Scroll.IsMouseCaptured)
        {
            Vector delta = e.GetPosition(this) - _lastMousePosition;
            Scroll.ScrollToHorizontalOffset(_lastHorizontalOffset - delta.X);
            Scroll.ScrollToVerticalOffset(_lastVerticalOffset - delta.Y);
        }
    }

    private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle)
        {
            Scroll.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        double factor = e.Delta > 0 ? ZoomStep : 1.0 / ZoomStep;
        
        SheetCanvas.Zoom = Math.Clamp(SheetCanvas.Zoom * factor, ZoomMin, ZoomMax);

        e.Handled = true;
    }

    private void CentreScroll()
    {
        Scroll.ScrollToHorizontalOffset((Scroll.ExtentWidth - Scroll.ViewportWidth)  / 2.0);
        Scroll.ScrollToVerticalOffset((Scroll.ExtentHeight - Scroll.ViewportHeight) / 2.0);
    }
}
