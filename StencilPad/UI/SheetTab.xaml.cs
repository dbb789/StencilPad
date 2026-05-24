using System.ComponentModel;
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

    private bool _snapToGrid = true;
    public bool SnapToGrid
    {
        get => _snapToGrid;
        set => _snapToGrid = value;
    }

    private bool _snapToPoint = false;
    public bool SnapToPoint
    {
        get => _snapToPoint;
        set => _snapToPoint = value;
    }

    private SheetTabViewModel? _viewModel;
    private bool _updatingZoom;

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

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is SheetTabViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
            oldVm.DetachCanvas();
        }

        _viewModel = e.NewValue as SheetTabViewModel;

        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.AttachCanvas(SheetCanvas);

            Dispatcher.BeginInvoke(() =>
            {
                SetZoom(_viewModel.Zoom);
                CentreScroll();
            });
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SheetTabViewModel.Zoom) && !_updatingZoom)
        {
            ApplyZoomCentred(_viewModel!.Zoom);
        }
    }

    private void SheetCanvasReady()
    {
        if (DataContext is SheetTabViewModel vm)
        {
            vm.AttachCanvas(SheetCanvas);
            _viewModel = vm;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        SheetCanvas.Viewport.ViewportChanged += () =>
        {
            Scroll.MaxWidth = SheetCanvas.Viewport.ToPixels(SheetCanvas.Viewport.Size.X) + 32;
            Scroll.MaxHeight = SheetCanvas.Viewport.ToPixels(SheetCanvas.Viewport.Size.Y) + 32;
        };

        Dispatcher.BeginInvoke(CentreScroll);
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

    private void ApplyZoomCentred(double targetZoom)
    {
        double hFraction = (Scroll.ScrollableWidth > 0) ? Scroll.HorizontalOffset / Scroll.ScrollableWidth  : 0.5;
        double vFraction = (Scroll.ScrollableHeight > 0) ? Scroll.VerticalOffset / Scroll.ScrollableHeight : 0.5;

        SetZoom(Math.Clamp(targetZoom, ZoomMin, ZoomMax));

        void OnLayoutUpdated(object? s, EventArgs e)
        {
            Scroll.LayoutUpdated -= OnLayoutUpdated;
            Scroll.ScrollToHorizontalOffset(hFraction * Scroll.ScrollableWidth);
            Scroll.ScrollToVerticalOffset(vFraction* Scroll.ScrollableHeight);
        }

        Scroll.LayoutUpdated += OnLayoutUpdated;
    }

    private void ApplyZoom(double targetZoom, double anchorX, double anchorY)
    {
        double newZoom = Math.Clamp(targetZoom, ZoomMin, ZoomMax);
        double actualFactor = newZoom / SheetCanvas.Zoom;

        double newHOffset = (Scroll.HorizontalOffset + anchorX) * actualFactor - anchorX;
        double newVOffset = (Scroll.VerticalOffset   + anchorY) * actualFactor - anchorY;

        SetZoom(newZoom);

        void OnLayoutUpdated(object? s, EventArgs e)
        {
            Scroll.LayoutUpdated -= OnLayoutUpdated;
            Scroll.ScrollToHorizontalOffset(newHOffset);
            Scroll.ScrollToVerticalOffset(newVOffset);
        }

        Scroll.LayoutUpdated += OnLayoutUpdated;
    }

    private void SetZoom(double zoom)
    {
        _updatingZoom = true;
        
        SheetCanvas.Zoom = zoom;

        if (_viewModel is not null)
        {
            _viewModel.Zoom = zoom;
        }
        
        _updatingZoom = false;
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        double factor = e.Delta > 0 ? ZoomStep : 1.0 / ZoomStep;
        var mousePos = e.GetPosition(Scroll);
        
        ApplyZoom(SheetCanvas.Zoom * factor, mousePos.X, mousePos.Y);

        e.Handled = true;
    }

    private void CentreScroll()
    {
        Scroll.ScrollToHorizontalOffset((Scroll.ExtentWidth - Scroll.ViewportWidth)  / 2.0);
        Scroll.ScrollToVerticalOffset((Scroll.ExtentHeight - Scroll.ViewportHeight) / 2.0);
    }
}
