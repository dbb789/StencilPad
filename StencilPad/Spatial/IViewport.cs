using System.Windows;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Spatial;

public interface IViewport
{
    Unit2D Size { get; }
    double Zoom { get; }

    event Action? ViewportChanged;
    
    double ToPixels(Unit unit);
    Point ToPoint(Unit2D position);
    Unit FromPixels(double pixels);
    Unit2D FromPixels(double pixelsX, double pixelsY);
    Unit2D FromPoint(Point point);

    Transform GetMillimetersToPixelsTransform();
}
