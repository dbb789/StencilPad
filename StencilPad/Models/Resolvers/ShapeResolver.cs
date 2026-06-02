using System.ComponentModel;
using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public class ShapeResolver : IStyledGeometryResolver
{
    private class PolygonState
    {
        public int Id;
        public bool HasStartCap;
        public bool HasEndCap;

        public PolygonState(int id)
        {
            Id = id;
        }
    }

    private readonly Shape _shape;
    private readonly GeometryStyle _style;
    private readonly List<IStyledGeometryWalker> _subscriptions;
    private readonly Dictionary<IPolygon, PolygonState> _polygonMap;
    private int _idCounter;
    
    public ShapeResolver(Shape shape)
    {
        _shape = shape;
        _style = GeometryStyle.ShapeDefault;
        _subscriptions = new();
        _polygonMap = new();
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
        foreach (var (polygon, state) in _polygonMap)
        {
            walker.AddResolver(state.Id,
                               polygon.Resolver,
                               _style,
                               _shape.Transform);

            if (state.HasStartCap)
            {
                walker.AddResource(StartCapId(state.Id),
                                   _shape.StartCap,
                                   _style,
                                   _shape.Transform * new UnitTransform(polygon.Vertices[0].Position));
            }

            if (state.HasEndCap)
            {
                walker.AddResource(EndCapId(state.Id),
                                   _shape.StartCap,
                                   _style,
                                   _shape.Transform * new UnitTransform(polygon.Vertices[^1].Position));
            }
        }
    }

    private void AddPolygon(IPolygon polygon)
    {
        var id = ++_idCounter;

        _polygonMap[polygon] = new PolygonState(id)
        {
            HasStartCap = HasStartCap(polygon),
            HasEndCap = HasEndCap(polygon)
        };

        polygon.GeometryChanged += GeometryChanged;

        foreach (var subscription in _subscriptions)
        {
            subscription.AddResolver(id,
                                     polygon.Resolver,
                                     _style,
                                     _shape.Transform);
        }
    }

    private void RemovePolygon(IPolygon polygon)
    {
        if (_polygonMap.TryGetValue(polygon, out var state))
        {
            _polygonMap.Remove(polygon);

            polygon.GeometryChanged -= GeometryChanged;

            foreach (var subscription in _subscriptions)
            {
                subscription.RemoveResolver(state.Id);
            }
        }
    }

    private void GeometryChanged(IPolygon polygon)
    {
        if (_polygonMap.TryGetValue(polygon, out var state))
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.UpdateResolver(state.Id, polygon.Resolver);

                UpdateStartCap(polygon, state);
                UpdateEndCap(polygon, state);
            }
        }
    }

    private void TransformChanged(ISheetElement element)
    {
        foreach (var (polygon, state) in _polygonMap)
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.UpdateResolver(state.Id, polygon.Resolver);

                if (state.HasStartCap)
                {
                    subscription.UpdateResource(StartCapId(state.Id),
                                                _style,
                                                _shape.Transform * new UnitTransform(polygon.Vertices[0].Position));
                }

                if (state.HasEndCap)
                {
                    subscription.UpdateResource(EndCapId(state.Id),
                                                _style,
                                                _shape.Transform * new UnitTransform(polygon.Vertices[^1].Position));
                }
            }
        }
    }

    private void PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        foreach (var (polygon, state) in _polygonMap)
        {
            foreach (var subscription in _subscriptions)
            {
                UpdateStartCap(polygon, state);
                UpdateEndCap(polygon, state);
            }
        }
    }

    private void UpdateStartCap(IPolygon polygon, PolygonState state)
    {
        var nextHasStartCap = HasStartCap(polygon);

        if (state.HasStartCap != nextHasStartCap)
        {
            state.HasStartCap = nextHasStartCap;

            foreach (var subscription in _subscriptions)
            {
                if (nextHasStartCap)
                {
                    subscription.AddResource(StartCapId(state.Id),
                                             _shape.StartCap,
                                             _style,
                                             _shape.Transform * new UnitTransform(polygon.Vertices[0].Position));
                }
                else
                {
                    subscription.RemoveResource(StartCapId(state.Id));
                }
            }
        }
        else
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.UpdateResource(StartCapId(state.Id),
                                            _style,
                                            _shape.Transform * new UnitTransform(polygon.Vertices[0].Position));
            }
        }
    }

    private void UpdateEndCap(IPolygon polygon, PolygonState state)
    {
        var nextHasEndCap = HasEndCap(polygon);

        if (state.HasEndCap != nextHasEndCap)
        {
            state.HasEndCap = nextHasEndCap;

            foreach (var subscription in _subscriptions)
            {
                if (nextHasEndCap)
                {
                    subscription.AddResource(EndCapId(state.Id),
                                             _shape.EndCap,
                                             _style,
                                             _shape.Transform * new UnitTransform(polygon.Vertices[^1].Position));
                }
                else
                {
                    subscription.RemoveResource(EndCapId(state.Id));
                }
            }
        }
        else
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.UpdateResource(EndCapId(state.Id),
                                            _style,
                                            _shape.Transform * new UnitTransform(polygon.Vertices[^1].Position));
            }
        }
    }
    
    private int StartCapId(int polygonId)
    {
        return polygonId * 2;
    }

    private int EndCapId(int polygonId)
    {
        return (polygonId * 2) + 1;
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
}
