using System.ComponentModel;
using StencilPad.Spatial;
using StencilPad.Services;

namespace StencilPad.Models.Resolvers;

public class MarkerPathResolver : IStyledGeometryResolver
{
    private const int PolygonId = 1;

    private readonly MarkerPath _markerPath;
    private readonly IResourceService _resourceService;
    private readonly List<IStyledGeometryWalker> _subscriptions;

    private GeometryStyle _style;
    private bool _disposed;

    public MarkerPathResolver(MarkerPath markerPath, IResourceService resourceService)
    {
        _markerPath = markerPath;
        _resourceService = resourceService;
        _subscriptions = new();
        _style = CreateStyle();

        _markerPath.GeometryChanged += OnGeometryChanged;
        _markerPath.TransformChanged += OnTransformChanged;
        _markerPath.PropertyChanged += OnPropertyChanged;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _markerPath.GeometryChanged -= OnGeometryChanged;
        _markerPath.TransformChanged -= OnTransformChanged;
        _markerPath.PropertyChanged -= OnPropertyChanged;

        foreach (var walker in _subscriptions)
        {
            walker.Destroy(PolygonId);
        }
    }

    public void Subscribe(IStyledGeometryWalker walker)
    {
        _subscriptions.Add(walker);
        VisitAll(walker);
    }

    public void Unsubscribe(IStyledGeometryWalker walker)
    {
        _subscriptions.Remove(walker);
    }

    public void VisitAll(IStyledGeometryWalker walker)
    {
        walker.SetStyle(_style);
        walker.SetTransform(_markerPath.Transform);
        walker.Create(PolygonId, CreateGeometrySet());
    }

    private void OnGeometryChanged(ISheetElement element)
    {
        var set = CreateGeometrySet();

        foreach (var walker in _subscriptions)
        {
            walker.Update(PolygonId, set);
        }
    }

    private void OnTransformChanged(ISheetElement element)
    {
        foreach (var walker in _subscriptions)
        {
            walker.SetTransform(_markerPath.Transform);
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsStyleProperty(e.PropertyName))
        {
            _style = CreateStyle();

            foreach (var walker in _subscriptions)
            {
                walker.SetStyle(_style);
            }
        }
        else if (e.PropertyName == nameof(MarkerPath.MarkerType))
        {
            // Marker geometry resource changed — rebuild overlays.
            var set = CreateGeometrySet();

            foreach (var walker in _subscriptions)
            {
                walker.Update(PolygonId, set);
            }
        }
    }

    private GeometrySet CreateGeometrySet()
    {
        var markerResource = _resourceService.Get(_markerPath.MarkerType);
        var overlays = new List<(GeometryResource, UnitTransform)>(_markerPath.PointList.Count);

        for (int i = 0; i < _markerPath.PointList.Count; ++i)
        {
            overlays.Add((markerResource, _markerPath.PointList[i]));
        }

        return new GeometrySet(_markerPath.Polygon.Resolver, overlays);
    }

    private GeometryStyle CreateStyle()
    {
        return new GeometryStyle
        {
            LineColor = _markerPath.LineColor
        };
    }

    private static bool IsStyleProperty(string? propertyName)
    {
        return propertyName == nameof(MarkerPath.LineColor);
    }
}
