using System.Collections.Specialized;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class ShapeRenderer : SheetElementRenderer
{
    public override Shape Element => _shape;
    public override UnitBounds SelectionBounds
    {
        get
        {
            if (_geometry is null)
            {
                return UnitBounds.Empty;
            }

            return UnitBounds.FromMinMax(
                new Unit2D(Unit.FromMillimeters(_geometry.Bounds.Left),
                           Unit.FromMillimeters(_geometry.Bounds.Top)),
                new Unit2D(Unit.FromMillimeters(_geometry.Bounds.Right),
                           Unit.FromMillimeters(_geometry.Bounds.Bottom)));
        }
    }

    private Shape _shape;
    private StreamGeometry? _geometry;

    public ShapeRenderer(Shape shape)
    {
        _shape = shape;
        _shape.EditablePolygon.PolygonChanged += RebuildGeometry;

        RebuildGeometry();
    }

    public override void Dispose()
    {
        _shape.EditablePolygon.PolygonChanged -= RebuildGeometry;
    }

    public override bool HitTest(Unit2D unit)
    {
        if (_geometry is null)
        {
            return false;
        }

        return _geometry.FillContains(unit.Millimeters);
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        if (_geometry is null)
        {
            return false;
        }

        var rect = new RectangleGeometry(bounds.Millimeters);

        return _geometry.FillContainsWithDetail(rect) != IntersectionDetail.Empty;
    }

    public override void Render(DrawingContext dc)
    {
        if (_geometry is null)
        {
            return;
        }

        var pen = new Pen(Brushes.Black, 0.2);
        var fill = Brushes.Transparent;

        // if (_shape.EditablePolygon.Closed)
        // {
        //     fill = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
        // }
        
        dc.DrawGeometry(fill, pen, _geometry);
    }

    private void RebuildGeometry()
    {
        _geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = _geometry.Open())
        {
            RendererUtil.AddToGeometry(ctx, _shape.EditablePolygon);
        }

        _geometry.Freeze();

        InvokeInvalidateVisual();
    }
}
