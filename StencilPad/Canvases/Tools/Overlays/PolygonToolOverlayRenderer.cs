using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;
using StencilPad.Rendering;

namespace StencilPad.Canvases.Tools.Overlays;

public class PolygonToolOverlayRenderer : IToolOverlayRenderer
{
    public static readonly IToolOverlayRendererFactory Factory = new FactoryImpl();
    
    private class FactoryImpl : IToolOverlayRendererFactory
    {
        public IToolOverlayRenderer? CreateOverlay(ISheetElement element)
        {
            if (element is IPolygonSheetElement polygonSheetElement)
            {
                return new PolygonToolOverlayRenderer(polygonSheetElement);
            }

            return null;
        }
    }
    
    private readonly IPolygonSheetElement _element;
    private readonly StreamGeometryWalker _walker;

    private Pen? _edgeOverlayPen;
    private Pen? _controlStemPen;
    private Transform? _transform;
    private StreamGeometry? _edgeOverlayGeometry;
    private StreamGeometry? _controlStemGeometry;
    private bool _geometryDirty;
    
    public event Action? RendererDirty;

    private PolygonToolOverlayRenderer(IPolygonSheetElement element)
    {
        _element = element;
        _element.PolygonSet.PolygonAdded += PolygonAdded;
        _element.PolygonSet.PolygonRemoved += PolygonRemoved;
        _element.PolygonSet.HandleSource.HandleSelectionChanged += SelectionChanged;
        _element.TransformChanged += TransformChanged;
        
        _walker = new();
        
        _edgeOverlayPen = new Pen(Brushes.Blue, 0.3);
        _edgeOverlayPen.Freeze();

        _controlStemPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 200, 0)), 0.2);
        _controlStemPen.Freeze();
        
        foreach (var polygon in _element.PolygonSet)
        {
            polygon.GeometryChanged += MarkGeometryDirty;
        }

        _transform = _element.Transform.CreateGroupTransform();

        RebuildGeometry();
        _geometryDirty = false;
    }

    public void Dispose()
    {
        foreach (var polygon in _element.PolygonSet)
        {
            polygon.GeometryChanged -= MarkGeometryDirty;
        }
        
        _element.PolygonSet.PolygonAdded -= PolygonAdded;
        _element.PolygonSet.PolygonRemoved -= PolygonRemoved;
        _element.PolygonSet.HandleSource.HandleSelectionChanged -= SelectionChanged;
        _element.TransformChanged -= TransformChanged;
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
        _transform = _element.Transform.CreateGroupTransform();
        
        InvokeRendererDirty();
    }
    
    private void RebuildGeometry()
    {
        var polygonList = _element.PolygonSet;

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

    public void Render(DrawingContext dc)
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

    private void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
