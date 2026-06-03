using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public readonly record struct GeometryStyle
{
    public GeometryStyle()
    {
        FillColor = Color.FromArgb(0, 255, 255, 255);
        LineColor = Color.FromArgb(255, 0, 0, 0);
        LineWidth = Unit.FromMillimeters(0.2);
        LineStyle = LineStyleResourceId.Solid;
    }
    
    public Color FillColor { get; init; }
    public Color LineColor { get; init; }
    public Unit LineWidth { get; init; }
    public LineStyleResourceId LineStyle { get; init; } = LineStyleResourceId.Solid;
}
