using System.ComponentModel;
using StencilPad.Spatial;
using StencilPad.Services;

namespace StencilPad.Models.Resolvers;

public class RulerResolver : IModelResolver
{
    private const int GeometryId = 1;

    private readonly Ruler _ruler;
    private readonly IResourceService _resourceService;
    private readonly LineResolver _lineResolver;
    private readonly List<(GeometryResource, UnitTransform)> _caps;

    private IModelWalker? _walker;
    private IStyledGeometryWalker? _geometryWalker;
    
    private GeometryStyle _style;

    public RulerResolver(Ruler ruler, IResourceService resourceService)
    {
        _ruler = ruler;
        _resourceService = resourceService;
        _lineResolver = new();
        _caps = new();
        _style = CreateStyle();

        _ruler.GeometryChanged += OnGeometryChanged;
        _ruler.TransformChanged += OnTransformChanged;
        _ruler.PropertyChanged += OnPropertyChanged;
    }

    public void Dispose()
    {
        Detach();
        
        _ruler.GeometryChanged -= OnGeometryChanged;
        _ruler.TransformChanged -= OnTransformChanged;
        _ruler.PropertyChanged -= OnPropertyChanged;
    }

    public void Attach(IModelWalker walker)
    {
        _walker = walker;
        _walker.SetTransform(_ruler.Transform);
        
        _geometryWalker = walker.CreateStyledGeometryWalker();
        _geometryWalker.SetStyle(_style);
        _geometryWalker.Create(GeometryId, CreateGeometrySet());
    }

    public void Detach()
    {
        _geometryWalker?.Destroy(GeometryId);
        _geometryWalker = null;
        _walker = null;
    }
    
    private void OnGeometryChanged(ISheetElement element)
    {
        _geometryWalker?.Update(GeometryId, CreateGeometrySet());
    }

    private void OnTransformChanged(ISheetElement element)
    {
        _walker?.SetTransform(_ruler.Transform);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsStyleProperty(e.PropertyName))
        {
            _style = CreateStyle();
            _geometryWalker?.SetStyle(_style);
        }
        else
        {
            _geometryWalker?.Update(GeometryId, CreateGeometrySet());
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
