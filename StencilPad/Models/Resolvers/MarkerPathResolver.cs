using System.ComponentModel;
using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public class MarkerPathResolver : IModelResolver
{
    private const int GeometryId = 1;

    private readonly MarkerPath _markerPath;
    private readonly IResourceSet _resourceSet;
    
    private IModelWalker? _walker;
    private IStyledGeometryWalker? _geometryWalker;

    private GeometryStyle _style;

    public MarkerPathResolver(MarkerPath markerPath, IResourceSet resourceSet)
    {
        _markerPath = markerPath;
        _resourceSet = resourceSet;
        _style = CreateStyle();

        _markerPath.GeometryChanged += OnGeometryChanged;
        _markerPath.TransformChanged += OnTransformChanged;
        _markerPath.PropertyChanged += OnPropertyChanged;
    }

    public void Dispose()
    {
        Detach();

        _markerPath.GeometryChanged -= OnGeometryChanged;
        _markerPath.TransformChanged -= OnTransformChanged;
        _markerPath.PropertyChanged -= OnPropertyChanged;
    }
    
    public void Attach(IModelWalker walker)
    {
        _walker = walker;
        _walker.SetTransform(_markerPath.Transform);
        
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
        _walker?.SetTransform(_markerPath.Transform);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsStyleProperty(e.PropertyName))
        {
            _style = CreateStyle();
            _geometryWalker?.SetStyle(_style);
        }
        else if (e.PropertyName == nameof(MarkerPath.MarkerType))
        {
            _geometryWalker?.Update(GeometryId, CreateGeometrySet());
        }
    }

    private GeometrySet CreateGeometrySet()
    {
        var markerResource = _resourceSet.Get(_markerPath.MarkerType);
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
