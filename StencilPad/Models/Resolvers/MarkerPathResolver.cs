using StencilPad.Spatial;
using System.ComponentModel;
using System.Windows.Shapes;

namespace StencilPad.Models.Resolvers;

public class MarkerPathResolver : SheetElementResolver
{
    private const int GeometryId = 1;

    private readonly MarkerPath _markerPath;
    private readonly IResourceSet _resourceSet;
    
    private IModelWalker? _walker;
    private IStyledGeometryWalker? _pathGeometryWalker;
    private IStyledGeometryWalker? _markerGeometryWalker;

    private GeometryStyle _pathStyle;
    private GeometryStyle _markerStyle;

    public MarkerPathResolver(MarkerPath markerPath, IResourceSet resourceSet)
        : base(markerPath)
    {
        _markerPath = markerPath;
        _resourceSet = resourceSet;
        _pathStyle = CreatePathStyle();
        _markerStyle = CreateMarkerStyle();

        _markerPath.GeometryChanged += OnGeometryChanged;
        _markerPath.TransformChanged += OnTransformChanged;
        _markerPath.PropertyChanged += OnPropertyChanged;
    }

    public override void Dispose()
    {
        Detach();

        _markerPath.GeometryChanged -= OnGeometryChanged;
        _markerPath.TransformChanged -= OnTransformChanged;
        _markerPath.PropertyChanged -= OnPropertyChanged;
    }
    public override UnitBounds GetOutlineBounds(UnitTransform transform)
    {
        var bounds = _markerPath.GetTransformedBounds(transform);
        var padding = _markerPath.LineWidth / 2;

        var markerResource = _resourceSet.Get(_markerPath.MarkerType);

        if (markerResource is not null)
        {
            padding = Unit.Max(padding, markerResource.Size.Magnitude / 2);
        }
        
        return bounds.Pad(padding);
    }

    public override bool OutlineContainsPoint(Unit2D point)
    {
        // Fast check against bounding box.
        if (!base.OutlineContainsPoint(point))
        {
            return false;
        }

        var localPoint = _markerPath.Transform.InverseApply(point);

        foreach (var polygon in _markerPath.PolygonSet)
        {
            if (PolygonUtil.ContainsPoint(polygon, localPoint, _markerPath.LineWidth))
            {
                return true;
            }
        }

        return false;
    }

    public override void Attach(IModelWalker walker)
    {
        _walker = walker;
        _walker.SetTransform(_markerPath.Transform);
        
        _pathGeometryWalker = walker.CreateStyledGeometryWalker();
        _pathGeometryWalker.SetStyle(_pathStyle);
        _pathGeometryWalker.Create(GeometryId, CreatePathGeometrySet());

        _markerGeometryWalker = walker.CreateStyledGeometryWalker();
        _markerGeometryWalker.SetStyle(_markerStyle);
        _markerGeometryWalker.Create(GeometryId, CreateMarkerGeometrySet());
    }

    public override void Detach()
    {
        _pathGeometryWalker?.Destroy(GeometryId);
        _pathGeometryWalker = null;

        _markerGeometryWalker?.Destroy(GeometryId);
        _markerGeometryWalker = null;
        
        _walker = null;
    }
    
    private void OnGeometryChanged(ISheetElement element)
    {
        _pathGeometryWalker?.Update(GeometryId, CreatePathGeometrySet());
        _markerGeometryWalker?.Update(GeometryId, CreateMarkerGeometrySet());

        InvokeOutlineChanged();
    }

    private void OnTransformChanged(ISheetElement element)
    {
        _walker?.SetTransform(_markerPath.Transform);

        InvokeOutlineChanged();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsPathStyleProperty(e.PropertyName))
        {
            _pathStyle = CreatePathStyle();
            _pathGeometryWalker?.SetStyle(_pathStyle);
        }
        else if (IsMarkerStyleProperty(e.PropertyName))
        {
            _markerStyle = CreateMarkerStyle();
            _markerGeometryWalker?.SetStyle(_markerStyle);
        }
        else if (e.PropertyName == nameof(MarkerPath.MarkerType))
        {
            _markerGeometryWalker?.Update(GeometryId, CreateMarkerGeometrySet());
        }

        InvokeOutlineChanged();
    }

    private GeometrySet CreatePathGeometrySet()
    {
        var markerResource = _resourceSet.Get(_markerPath.MarkerType);

        return new GeometrySet(_markerPath.Polygon.Resolver);
    }

    private GeometrySet CreateMarkerGeometrySet()
    {
        var markerResource = _resourceSet.Get(_markerPath.MarkerType);
        var overlays = new List<(GeometryResource, UnitTransform)>(_markerPath.PointList.Count);

        for (int i = 0; i < _markerPath.PointList.Count; ++i)
        {
            overlays.Add((markerResource, _markerPath.PointList[i]));
        }

        return new GeometrySet(EmptyGeometryResolver.Instance, overlays);
    }

    private GeometryStyle CreatePathStyle()
    {
        return new GeometryStyle
        {
            LineColor = _markerPath.LineColor,
            LineWidth = _markerPath.LineWidth
        };
    }

    private GeometryStyle CreateMarkerStyle()
    {
        return new GeometryStyle
        {
            LineColor = _markerPath.MarkerColor,
        };
    }

    private static bool IsPathStyleProperty(string? propertyName)
    {
        return propertyName == nameof(MarkerPath.LineColor) ||
            propertyName == nameof(MarkerPath.LineWidth);
    }

    private static bool IsMarkerStyleProperty(string? propertyName)
    {
        return propertyName == nameof(MarkerPath.MarkerColor);
    }
}
