using System.ComponentModel;
using StencilPad.Spatial;
using StencilPad.Services;

namespace StencilPad.Models.Resolvers;

public class ShapeResolver : IStyledGeometryResolver
{
    private class PolygonState
    {
        public int Id;
        
        public PolygonState(int id)
        {
            Id = id;
        }
    }

    private readonly Shape _shape;
    private readonly IResourceService _resourceService;
    private readonly List<IStyledGeometryWalker> _subscriptions;
    private readonly Dictionary<IPolygon, PolygonState> _polygonMap;

    private CapDistanceWalker? _capDistanceWalker;
    private GeometryStyle _style;
    private int _idCounter;
    
    public ShapeResolver(Shape shape, IResourceService resourceService)
    {
        _shape = shape;
        _resourceService = resourceService;
        _subscriptions = new();
        _polygonMap = new();
        _style = CreateStyle();
        _idCounter = 0;

        for (int i = 0; i < _shape.PolygonSet.Count; ++i)
        {
            AddPolygon(_shape.PolygonSet[i]);
        }

        _shape.TransformChanged += TransformChanged;
        _shape.PropertyChanged += PropertyChanged;
        _shape.PolygonSet.PolygonAdded += AddPolygon;
        _shape.PolygonSet.PolygonRemoved += RemovePolygon;
    }

    public void Dispose()
    {
        _shape.TransformChanged -= TransformChanged;
        _shape.PropertyChanged -= PropertyChanged;
        _shape.PolygonSet.PolygonAdded -= AddPolygon;
        _shape.PolygonSet.PolygonRemoved -= RemovePolygon;

        foreach (var polygon in _shape.PolygonSet)
        {
            RemovePolygon(polygon);
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
        walker.SetTransform(_shape.Transform);
        
        foreach (var (polygon, state) in _polygonMap)
        {
            walker.Create(state.Id, 
                          CreateGeometrySet(polygon));
        }
    }

    private void AddPolygon(IPolygon polygon)
    {
        var id = ++_idCounter;

        _polygonMap[polygon] = new PolygonState(id);
        polygon.GeometryChanged += GeometryChanged;

        foreach (var walker in _subscriptions)
        {
            walker.Create(id, CreateGeometrySet(polygon));
        }

    }

    private void RemovePolygon(IPolygon polygon)
    {
        if (!_polygonMap.TryGetValue(polygon, out var state))
        {
            return;
        }
        
        _polygonMap.Remove(polygon);
        polygon.GeometryChanged -= GeometryChanged;

        foreach (var walker in _subscriptions)
        {
            walker.Destroy(state.Id);
        }
    }

    private void GeometryChanged(IPolygon polygon)
    {
        if (_polygonMap.TryGetValue(polygon, out var state))
        {
            foreach (var walker in _subscriptions)
            {
                walker.Update(state.Id, CreateGeometrySet(polygon));
            }
        }
    }

    private void TransformChanged(ISheetElement element)
    {
        foreach (var walker in _subscriptions)
        {
            walker.SetTransform(_shape.Transform);
        }
    }

    private void PropertyChanged(object? sender, PropertyChangedEventArgs e)
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
            foreach (var (polygon, state) in _polygonMap)
            {
                foreach (var walker in _subscriptions)
                {
                    walker.Update(state.Id, CreateGeometrySet(polygon));
                }
            }
        }
    }

    private GeometrySet CreateGeometrySet(IPolygon polygon)
    {
        var caps = new List<(GeometryResource, UnitTransform)>();

        var startCap = HasStartCap(polygon) ? _resourceService.Get(_shape.StartCap) : null;
        var endCap = HasEndCap(polygon) ? _resourceService.Get(_shape.EndCap) : null;

        SegmentPoint? startPoint = null;
        SegmentPoint? endPoint = null;

        if (startCap is not null)
        {
            _capDistanceWalker ??= new CapDistanceWalker();
            _capDistanceWalker.Reset(startCap.Size.Y + _style.LineWidth);
            
            polygon.Resolver.Walk(_capDistanceWalker);
            
            startPoint = _capDistanceWalker.Point;

            caps.Add((startCap, BuildCapTransform(polygon.Vertices[0].Position,
                                                  _capDistanceWalker.Position)));
        }

        if (endCap is not null)
        {
            _capDistanceWalker ??= new CapDistanceWalker();
            _capDistanceWalker.Reset(endCap.Size.Y + _style.LineWidth);

            polygon.Resolver.WalkReverse(_capDistanceWalker);

            endPoint = _capDistanceWalker.Point;

            if (endPoint is not null)
            {
                endPoint = endPoint.Value with { Fraction = 1.0 - endPoint.Value.Fraction };
            }

            caps.Add((endCap, BuildCapTransform(polygon.Vertices[^1].Position,
                                                _capDistanceWalker.Position)));
        }

        return new GeometrySet(polygon.Resolver,
                               startPoint,
                               endPoint,
                               caps);
    }

    private bool IsStyleProperty(string? propertyName)
    {
        return propertyName == nameof(Shape.LineColor) ||
               propertyName == nameof(Shape.LineWidth) ||
               propertyName == nameof(Shape.LineStyle) ||
               propertyName == nameof(Shape.FillColor);
    }
    
    private GeometryStyle CreateStyle()
    {
        return new GeometryStyle
        {
            LineColor = _shape.LineColor,
            LineWidth = _shape.LineWidth,
            LineStyle = _shape.LineStyle,
            FillColor = _shape.FillColor
        };
    }

    private bool HasStartCap(IPolygon polygon)
    {
        return !polygon.Closed &&
            polygon.Vertices.Count > 1 &&
            _shape.StartCap != GeometryResourceId.None;
    }

    private bool HasEndCap(IPolygon polygon)
    {
        return !polygon.Closed &&
            polygon.Vertices.Count > 1 &&
            _shape.EndCap != GeometryResourceId.None;
    }

    private UnitTransform BuildCapTransform(Unit2D basePosition, Unit2D offsetPosition)
    {
        var offset = basePosition - offsetPosition;

        basePosition -= offset.NormalizedTo(_style.LineWidth);

        var rotation = Math.Atan2(offset.Y.Millimeters,
                                  offset.X.Millimeters) * MathUtil.Rad2Deg;

        return new UnitTransform(basePosition, (decimal)rotation + 90);
    }
}
