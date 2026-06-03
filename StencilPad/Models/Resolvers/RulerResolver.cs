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
    private readonly LineResolver _lineResolver;
    private readonly List<(GeometryResource, UnitTransform)> _caps;

    private GeometryStyle _style;

    public RulerResolver(Ruler markerPath, IResourceService resourceService)
    {
        _ruler = markerPath;
        _resourceService = resourceService;
        _subscriptions = new();
        _lineResolver = new();
        _caps = new();
        _style = CreateStyle();

        _ruler.GeometryChanged += OnGeometryChanged;
        _ruler.TransformChanged += OnTransformChanged;
        _ruler.PropertyChanged += OnPropertyChanged;
    }

    public void Dispose()
    {
        _ruler.GeometryChanged -= OnGeometryChanged;
        _ruler.TransformChanged -= OnTransformChanged;
        _ruler.PropertyChanged -= OnPropertyChanged;

        foreach (var walker in _subscriptions)
        {
            walker.Destroy(GeometryId);
        }

        _subscriptions.Clear();
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
        _lineResolver.Line = new Line(_ruler.Min, _ruler.Max);

        _caps.Clear();
        _caps.Add((_resourceService.Get(GeometryResourceId.First), new UnitTransform(_ruler.Min)));
        _caps.Add((_resourceService.Get(GeometryResourceId.First), new UnitTransform(_ruler.Max)));
        
        return new GeometrySet(_lineResolver, _caps);
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
