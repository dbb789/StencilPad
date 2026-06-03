using System.ComponentModel;
using StencilPad.Spatial;
using StencilPad.Services;

namespace StencilPad.Models.Resolvers;

public class RulerResolver : IStyledGeometryResolver
{
    private const int GeometryId = 1;

    private readonly Ruler _ruler;
    private readonly IResourceService _resourceService;
    private readonly List<IStyledGeometryWalker> _subscriptions;

    private GeometryStyle _style;
    private bool _disposed;

    public RulerResolver(Ruler markerPath, IResourceService resourceService)
    {
        _ruler = markerPath;
        _resourceService = resourceService;
        _subscriptions = new();
        _style = CreateStyle();

        _ruler.GeometryChanged += OnGeometryChanged;
        _ruler.TransformChanged += OnTransformChanged;
        _ruler.PropertyChanged += OnPropertyChanged;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _ruler.GeometryChanged -= OnGeometryChanged;
        _ruler.TransformChanged -= OnTransformChanged;
        _ruler.PropertyChanged -= OnPropertyChanged;

        foreach (var walker in _subscriptions)
        {
            walker.Destroy(GeometryId);
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
        walker.SetTransform(_ruler.Transform);
        walker.Create(GeometryId, CreateGeometrySet());
    }

    private void OnGeometryChanged(ISheetElement element)
    {
        var set = CreateGeometrySet();

        foreach (var walker in _subscriptions)
        {
            walker.Update(GeometryId, set);
        }
    }

    private void OnTransformChanged(ISheetElement element)
    {
        foreach (var walker in _subscriptions)
        {
            walker.SetTransform(_ruler.Transform);
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
        else
        {
            foreach (var walker in _subscriptions)
            {
                walker.Update(GeometryId, CreateGeometrySet());
            }
        }
    }

    private GeometrySet CreateGeometrySet()
    {
        var line = new Line(_ruler.Min, _ruler.Max);
        var lineResolver = new LineResolver(line);
        
        return new GeometrySet(lineResolver);
    }

    private GeometryStyle CreateStyle()
    {
        return new GeometryStyle
        {
            LineColor = _ruler.Color
        };
    }

    private static bool IsStyleProperty(string? propertyName)
    {
        return propertyName == nameof(Ruler.Color);
    }
}
