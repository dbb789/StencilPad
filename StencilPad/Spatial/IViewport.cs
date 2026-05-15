using System.Windows;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Spatial;

public interface IViewport
{
    Unit2D SheetSize { get; }
    Unit2D Size { get; }
    double Zoom { get; }

    event Action? ViewportChanged;
    
    Point ToPoint(Unit2D position);
    Unit2D FromPixels(double pixelsX, double pixelsY);
    Unit2D FromPoint(Point point);
    Unit FromPixels(double pixels);
    double ToPixels(Unit unit);

    Transform GetMillimetersToPixelsTransform();
}
