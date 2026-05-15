using System.Windows;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class VisualViewport() : IViewport
{
    private const double MmPerInch = 25.4;

    public Visual? Visual
    {
        get => _visual;
        set
        {
            if (_visual == value)
            {
                return;
            }

            _visual = value;
            _dpi = (_visual != null) ? VisualTreeHelper.GetDpi(_visual).PixelsPerInchX : 96.0;
            
            ViewportChanged?.Invoke();
        }
    }

    public Unit2D SheetSize
    {
        get => _sheetSize;
        set
        {
            if (_sheetSize == value)
            {
                return;
            }

            _sheetSize = value;
            ViewportChanged?.Invoke();
        }
    }

    public Unit2D Size
    {
        get => _size;
        set
        {
            if (_size == value)
            {
                return;
            }

            if (value.X <= Unit.Zero || value.Y <= Unit.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Size dimensions must be positive.");
            }

            _size = value;

            ViewportChanged?.Invoke();
        }
    }


    public double Zoom
    {
        get => _zoom;
        set
        {
            if (_zoom == value)
            {
                return;
            }

            if (value <= 0.0 || double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Zoom must be positive.");
            }

            _zoom = value;

            ViewportChanged?.Invoke();
        }
    }

    private Unit2D _sheetSize = new Unit2D(Unit.FromMillimeters(210.0),
                                           Unit.FromMillimeters(297.0));

    private Unit2D _size = new Unit2D(Unit.FromMillimeters(410.0),
                                      Unit.FromMillimeters(497.0));

    private double _zoom = 1.0;

    private Visual? _visual = null;
    private double _dpi = 96.0;
    
    public event Action? ViewportChanged;

    public double ToPixels(Unit unit)
    {
        return unit.Millimeters / MmPerInch * _dpi * Zoom;
    }

    public Point ToPoint(Unit2D position)
    {
        return new Point(ToPixels(position.X) + ToPixels(Size.X) / 2.0,
                         ToPixels(position.Y) + ToPixels(Size.Y) / 2.0);
    }

    public Unit FromPixels(double pixels)
    {
        return Unit.FromMillimeters(pixels * MmPerInch / _dpi / Zoom);
    }

    public Unit2D FromPixels(double pixelsX, double pixelsY)
    {
        return new Unit2D(FromPixels(pixelsX), FromPixels(pixelsY));
    }

    public Unit2D FromPoint(Point point)
    {
        return new Unit2D(FromPixels(point.X - ToPixels(Size.X) / 2.0),
                          FromPixels(point.Y - ToPixels(Size.Y) / 2.0));
    }

    public Transform GetMillimetersToPixelsTransform()
    {
        var scale = ToPixels(Unit.FromMillimeters(1.0));
        var transform = new TransformGroup();

        transform.Children.Add(new TranslateTransform(Size.X.Millimeters / 2.0,
                                                      Size.Y.Millimeters / 2.0));
        transform.Children.Add(new ScaleTransform(scale, scale));

        return transform;
    }
}
