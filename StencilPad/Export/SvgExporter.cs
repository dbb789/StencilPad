using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Xml.Linq;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Export;

public static class SvgExporter
{
    private static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";

    public static void Export(Sheet sheet, string path)
    {
        UnitBounds? sheetBounds = null;

        foreach (var element in sheet.Elements)
        {
            sheetBounds = UnitBounds.Union(sheetBounds, element.GetTransformedBounds());
        }

        var bounds = sheetBounds ??
            UnitBounds.FromCenterSize(Unit2D.Zero,
                                      new Unit2D(Unit.FromMillimeters(98),
                                                 Unit.FromMillimeters(98)));

        bounds = bounds.Pad(Unit.FromMillimeters(1));

        double width   = bounds.Size.X.Millimeters;
        double height  = bounds.Size.Y.Millimeters;
        double offsetX = bounds.Min.X.Millimeters;
        double offsetY = bounds.Min.Y.Millimeters;

        var svg = new XElement(SvgNs + "svg",
            new XAttribute("width",   Mm(width)),
            new XAttribute("height",  Mm(height)),
            new XAttribute("viewBox", $"{Num(offsetX)} {Num(offsetY)} {Num(width)} {Num(height)}"));

        foreach (var element in sheet.Elements)
        {
            if (element is Shape shape)
            {
                foreach (var pathElement in BuildShapeElements(shape))
                {
                    svg.Add(pathElement);
                }
            }
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), svg);

        using var stream = File.OpenWrite(path);
        doc.Save(stream);
    }

    private static IEnumerable<XElement> BuildShapeElements(Shape shape)
    {
        foreach (var polygon in shape.PolygonSet)
        {
            var pathData = BuildPathData(polygon, shape.Transform);

            if (string.IsNullOrEmpty(pathData))
            {
                continue;
            }

            var fill        = shape.FillColor.A == 0 ? "none" : ColorToSvg(shape.FillColor);
            var stroke      = ColorToSvg(shape.LineColor);
            var strokeWidth = Num(shape.LineWidth.Millimeters);

            var path = new XElement(SvgNs + "path",
                new XAttribute("d",              pathData),
                new XAttribute("fill",           fill),
                new XAttribute("stroke",         stroke),
                new XAttribute("stroke-width",   strokeWidth),
                new XAttribute("stroke-opacity", Num(shape.LineColor.A / 255.0)));

            if (shape.LineStyle == LineStyleResourceId.Dashes)
            {
                var dash = shape.LineWidth.Millimeters;
                path.Add(new XAttribute("stroke-dasharray", $"{Num(dash * 4)} {Num(dash * 2)}"));
            }

            yield return path;
        }
    }

    private static string BuildPathData(IPolygon polygon, UnitTransform transform)
    {
        if (polygon.Vertices.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        var first = transform.Apply(polygon.Vertices[0].Position);
        sb.Append($"M {Num(first.X.Millimeters)},{Num(first.Y.Millimeters)}");

        for (int i = 0; i < polygon.Edges.Count; i++)
        {
            var edge       = polygon.Edges[i];
            var fromVertex = polygon.Vertices.At(i);
            var toVertex   = polygon.Vertices.At((i + 1) % polygon.Vertices.Count);
            var to         = transform.Apply(toVertex.Position);

            if (edge.Type == EdgeType.Bezier)
            {
                var cp1 = transform.Apply(fromVertex.Position + edge.ControlBeginOffset);
                var cp2 = transform.Apply(toVertex.Position   + edge.ControlEndOffset);

                sb.Append($" C {Num(cp1.X.Millimeters)},{Num(cp1.Y.Millimeters)}");
                sb.Append($" {Num(cp2.X.Millimeters)},{Num(cp2.Y.Millimeters)}");
                sb.Append($" {Num(to.X.Millimeters)},{Num(to.Y.Millimeters)}");
            }
            else
            {
                sb.Append($" L {Num(to.X.Millimeters)},{Num(to.Y.Millimeters)}");
            }
        }

        if (polygon.Closed)
        {
            sb.Append(" Z");
        }

        return sb.ToString();
    }

    private static string ColorToSvg(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static string Mm(double value) => $"{Num(value)}mm";

    private static string Num(double value) => value.ToString("G6", CultureInfo.InvariantCulture);
}
