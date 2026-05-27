using System.ComponentModel;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class ShapeEditRenderer : SheetElementEditRenderer
{
    private readonly Shape _shape;
    private readonly StreamGeometryWalker _walker;

    private Pen? _edgeOverlayPen;
    private Pen? _controlStemPen;
    private Transform? _transform;
    private StreamGeometry? _edgeOverlayGeometry;
    private StreamGeometry? _controlStemGeometry;
    private bool _geometryDirty;

    public ShapeEditRenderer(Shape shape)
    {
        _shape = shape;
        _shape.PolygonSet.PolygonAdded += PolygonAdded;
        _shape.PolygonSet.PolygonRemoved += PolygonRemoved;
        _shape.PolygonSet.HandleSource.HandleSelectionChanged += SelectionChanged;
        _shape.TransformChanged += TransformChanged;
        
        _walker = new();
        
        _edgeOverlayPen = new Pen(Brushes.Blue, 0.3);
        _edgeOverlayPen.Freeze();

        _controlStemPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 200, 0)), 0.2);
        _controlStemPen.Freeze();
        
        foreach (var polygon in _shape.PolygonSet)
        {
            polygon.GeometryChanged += MarkGeometryDirty;
        }

        _transform = _shape.Transform.CreateGroupTransform();

        RebuildGeometry();
        _geometryDirty = false;
    }

    public override void Dispose()
    {
        foreach (var polygon in _shape.PolygonSet)
        {
            polygon.GeometryChanged -= MarkGeometryDirty;
        }
        
        _shape.PolygonSet.PolygonAdded -= PolygonAdded;
        _shape.PolygonSet.PolygonRemoved -= PolygonRemoved;
        _shape.PolygonSet.HandleSource.HandleSelectionChanged -= SelectionChanged;
        _shape.TransformChanged -= TransformChanged;
    }

    private void PolygonAdded(EditablePolygon polygon)
    {
        polygon.GeometryChanged += MarkGeometryDirty;
        MarkGeometryDirty();
    }

    private void PolygonRemoved(EditablePolygon polygon)
    {
        polygon.GeometryChanged -= MarkGeometryDirty;
        MarkGeometryDirty();
    }

    private void SelectionChanged(IHandleSource source, Handle handle, bool selected)
    {
        MarkGeometryDirty();
    }
    
    private void MarkGeometryDirty(IPolygon polygon)
    {
        MarkGeometryDirty();
    }

    private void MarkGeometryDirty()
    {
        _geometryDirty = true;

        InvokeRendererDirty();
    }

    private void TransformChanged(ISheetElement element)
    {
        _transform = _shape.Transform.CreateGroupTransform();
        
        InvokeRendererDirty();
    }
    
    private void RebuildGeometry()
    {
        var polygonList = _shape.PolygonSet;

        _edgeOverlayGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

        using (var ctx = _edgeOverlayGeometry.Open())
        {
            _walker.Context = ctx;
            
            foreach (var polygon in polygonList)
            {
                foreach (var edgeIndex in polygon.GetSelectedEdges())
                {
                    polygon.Resolver.WalkEdge(_walker, edgeIndex);
                }
            }
        }

        _edgeOverlayGeometry.Freeze();

        _controlStemGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

        using (var ctx = _controlStemGeometry.Open())
        {
            foreach (var polygon in polygonList)
            {
                for (int i = 0; i < polygon.Edges.Count; i++)
                {
                    var edge = polygon.Edges[i];

                    if (edge.Type == EdgeType.Bezier)
                    {
                        var vertexBegin = polygon.Vertices[i].Position;
                        var controlBegin = vertexBegin + edge.ControlBeginOffset;

                        ctx.BeginFigure(vertexBegin.Millimeters, isFilled: false, isClosed: false);
                        ctx.LineTo(controlBegin.Millimeters, isStroked: true, isSmoothJoin: false);

                        var vertexEnd = polygon.Vertices.At(i + 1).Position;
                        var controlEnd = vertexEnd + edge.ControlEndOffset;

                        ctx.BeginFigure(vertexEnd.Millimeters, isFilled: false, isClosed: false);
                        ctx.LineTo(controlEnd.Millimeters, isStroked: true, isSmoothJoin: false);
                    }
                }
            }
        }

        _controlStemGeometry.Freeze();
    }

    public override void Render(DrawingContext dc)
    {
        if (_geometryDirty)
        {
            _geometryDirty = false;
            RebuildGeometry();
        }

        if (_transform is null)
        {
            return;
        }
        
        dc.PushTransform(_transform);
        
        if (_edgeOverlayGeometry is not null)
        {
            dc.DrawGeometry(Brushes.Transparent,
                            _edgeOverlayPen,
                            _edgeOverlayGeometry);
        }

        if (_controlStemGeometry is not null)
        {
            dc.DrawGeometry(Brushes.Transparent,
                            _controlStemPen,
                            _controlStemGeometry);
        }

        dc.Pop();
    }
}
