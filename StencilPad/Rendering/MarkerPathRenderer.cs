using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class MarkerPathRenderer : SheetElementRenderer
{
    private record struct MarkerData(Point Position, int SegmentIndex);
    
    public override MarkerPath Element => _markerPath;

    public int MarkerCount => _markerCount;
    
    private const double MarkerHalfLengthMm = 1.0;

    private readonly MarkerPath _markerPath;
    private readonly IResourceService _resourceService;
    private StreamGeometry? _geometry;
    private int _markerCount;
    private Transform? _transform;
    
    public MarkerPathRenderer(MarkerPath markerPath, IResourceService resourceService)
    {
        _markerPath = markerPath;
        _markerPath.Polygon.GeometryChanged += RebuildGeometry;
        _markerPath.TransformChanged += OnTransformChanged;
        _markerPath.PropertyChanged += PropertyChanged;
        _markerCount = 0;

        _resourceService = resourceService;
        _transform = _markerPath.Transform.CreateGroupTransform();
        
        RebuildGeometry();
    }

    public override void Dispose()
    {
        _markerPath.Polygon.GeometryChanged -= RebuildGeometry;
        _markerPath.TransformChanged -= OnTransformChanged;
        _markerPath.PropertyChanged -= PropertyChanged;
    }

    public override void Render(DrawingContext dc)
    {
        if (_geometry is null || _transform is null)
        {
            return;
        }

        var pen = new Pen(Brushes.Black, 0.2);

        dc.PushTransform(_transform);
        dc.DrawGeometry(null, pen, _geometry);

        var markerGeometry = _resourceService.Get(_markerPath.MarkerType).Geometry;

        for (int i = 0; i < _markerPath.PointList.Count; ++i)
        {
            var t = _markerPath.PointList[i];
            var position = new Point(t.Position.X.Millimeters, t.Position.Y.Millimeters);
            
            dc.PushTransform(new TranslateTransform(position.X, position.Y));
            dc.PushTransform(new RotateTransform((double)t.Angle, 0, 0));
            dc.DrawGeometry(null, pen, markerGeometry);

            if ((i == _markerPath.PointList.Count - 1) && _markerPath.HasBalancePoint)
            {
                dc.DrawEllipse(null, pen, new Point(0, 0), 1, 1);
            }

            dc.Pop();
            dc.Pop();
        }

        dc.Pop();
    }

    private void PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RebuildGeometry();
        InvokeRendererDirty();
    }

    private void OnTransformChanged(ISheetElement element)
    {
        _transform = _markerPath.Transform.CreateGroupTransform();

        InvokeRendererDirty();
    }
    
    private void RebuildGeometry(IPolygon polygon)
    {
        RebuildGeometry();
    }
    
    private void RebuildGeometry()
    {
        _geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = _geometry.Open())
        {
            RendererUtil.AddToGeometry(ctx, _markerPath.Polygon);
        }

        _geometry.Freeze();

        InvokeRendererDirty();
    }
}
