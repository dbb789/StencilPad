using System.Windows;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Spatial;

public interface IViewport
{
    Unit2D SheetSize { get; }
    Unit2D Size { get; }
    double Zoom { get; }
    Transform MillimetersToPixelsTransform { get; }
    
    event Action? ViewportChanged;
    
    double ToPixels(Unit unit);
    Point ToPoint(Unit2D position);
    Rect ToRect(UnitBounds bounds);
    Unit FromPixels(double pixels);
    Unit2D FromVector(Vector vector);
    Unit2D FromPoint(Point point);
}
