using System.Windows;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class VisualViewport : IViewport
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
            
            OnViewportChanged();
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
            OnViewportChanged();
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
            
            OnViewportChanged();
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

            OnViewportChanged();
        }
    }
    
    public Transform MillimetersToPixelsTransform
    {
        get => _millimetersToPixelsTransform;
    }

    private Visual? _visual = null;
    private Unit2D _sheetSize;
    private Unit2D _size;
    private Vector _halfPixelSize;
    private double _zoom;
    private double _dpi;
    private Transform _millimetersToPixelsTransform;
    
    public event Action? ViewportChanged;

    public VisualViewport()
    {
        _visual = null;
        _sheetSize = new Unit2D(Unit.FromMillimeters(210.0), Unit.FromMillimeters(297.0));
        _size = _sheetSize * 1.1;
        _zoom = 1.0;
        _dpi = 96.0;
        _millimetersToPixelsTransform = GetMillimetersToPixelsTransform();

        OnViewportChanged();
    }
    
    public double ToPixels(Unit unit)
    {
        return unit.Millimeters / MmPerInch * _dpi * Zoom;
    }

    public Point ToPoint(Unit2D position)
    {
        return new Point(ToPixels(position.X), ToPixels(-position.Y)) + _halfPixelSize;
    }

    public Rect ToRect(UnitBounds bounds)
    {
        var topLeft = ToPoint(bounds.Min);
        var bottomRight = ToPoint(bounds.Max);

        return new Rect(topLeft, bottomRight);
    }
    
    public Unit FromPixels(double pixels)
    {
        return Unit.FromMillimeters(pixels * MmPerInch / _dpi / Zoom);
    }

    public Unit2D FromVector(Vector vector)
    {
        return new Unit2D(FromPixels(vector.X),
                          -FromPixels(vector.Y));
    }

    public Unit2D FromPoint(Point point)
    {
        return new Unit2D(FromPixels(point.X - _halfPixelSize.X),
                          -FromPixels(point.Y - _halfPixelSize.Y));
    }

    private void OnViewportChanged()
    {
        _millimetersToPixelsTransform = GetMillimetersToPixelsTransform();
        _halfPixelSize = new Vector(ToPixels(_size.X) / 2.0, ToPixels(_size.Y) / 2.0);
        
        ViewportChanged?.Invoke();
    }
    
    private Transform GetMillimetersToPixelsTransform()
    {
        var scale = ToPixels(Unit.FromMillimeters(1.0));
        var transform = new TransformGroup();

        transform.Children.Add(new TranslateTransform(_size.X.Millimeters / 2.0,
                                                      -_size.Y.Millimeters / 2.0));
        transform.Children.Add(new ScaleTransform(scale, -scale));

        transform.Freeze();
        
        return transform;
    }
}
