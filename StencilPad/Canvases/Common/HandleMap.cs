using System.Collections.Specialized;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public class HandleMap : IHandleMap, IUnitSnap
{
    public Sheet? Sheet
    {
        get => _sheet;
        set => SetSheet(value);
    }
    
    private Sheet? _sheet;
    private Dictionary<Handle, Unit2D> _byHandle;
    private QuadTree<HandleMapEntry> _byPosition;
    private List<(HandleMapEntry, Unit2D)> _queryResults;

    public event Action<IHandleSource, Handle, Unit2D>? HandleAdded;
    public event Action<IHandleSource, Handle>? HandleRemoved;
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    
    public HandleMap()
    {
        var pageSize = new Unit2D(Unit.FromMillimeters(1000), Unit.FromMillimeters(1000));

        _sheet = null;
        _byHandle = new();
        _byPosition = new QuadTree<HandleMapEntry>(UnitBounds.FromCenterSize(Unit2D.Zero, pageSize),
                                                   nodeCapacity: 64,
                                                   maxDepth: 64);
        _queryResults = [];
    }

    public void QueryHandles(UnitBounds bounds, List<(HandleMapEntry, Unit2D)> results)
    {
        _byPosition.Query(bounds, results);
    }
    
    private void SetSheet(Sheet? sheet)
    {
        if (_sheet == sheet)
        {
            return;
        }
        
        if (_sheet is not null)
        {
            _sheet.Elements.CollectionChanged -= SheetElementsChanged;
        }
        
        _sheet = sheet;
        
        if (_sheet is not null)
        {
            foreach (var element in _sheet.Elements)
            {
                Add(element);
            }
            
            _sheet.Elements.CollectionChanged += SheetElementsChanged;
        }
    }

    public Unit2D UnitSnap(Unit2D point)
    {
        _queryResults.Clear();
        _byPosition.Query(UnitBounds.FromCenterSize(point, new Unit2D(Unit.FromMillimeters(5),
                                                                      Unit.FromMillimeters(5))),
                          _queryResults);

        Unit2D? closestSnap = null;
        Unit closestDistance = Unit.FromMillimeters(5);
        
        foreach (var (handle, handlePosition) in _queryResults)
        {
            var distance = (point - handlePosition).Magnitude;
            
            if (closestSnap is null || distance < closestDistance)
            {
                closestSnap = handlePosition;
                closestDistance = distance;
            }
        }

        return closestSnap ?? point;
    }

    private void SheetElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (SheetElement element in e.NewItems)
            {
                Add(element);
            }
        }

        if (e.OldItems is not null)
        {
            foreach (SheetElement element in e.OldItems)
            {
                Remove(element);
            }
        }
    }

    private void Add(ISheetElement element)
    {
        foreach (var handle in element.HandleSource.Handles)
        {
            Add(element, handle, element.HandleSource.GetPoint(handle));    
        }
        
        element.HandleSource.HandleMoved += OnHandleMoved;
    }

    private void Remove(ISheetElement element)
    {
        foreach (var handle in element.HandleSource.Handles)
        {
            Remove(element, handle);
        }
        
        element.HandleSource.HandleMoved -= OnHandleMoved;
    }

    private void Add(ISheetElement element, Handle handle, Unit2D position)
    {
        _byHandle[handle] = position;
        _byPosition.Insert(position, new HandleMapEntry(element.HandleSource, handle));
        HandleAdded?.Invoke(element.HandleSource, handle, position);
    }

    private void Remove(ISheetElement element, Handle handle)
    {
        if (_byHandle.TryGetValue(handle, out var position))
        {
            _byPosition.Remove(position, new HandleMapEntry(element.HandleSource, handle));
            _byHandle.Remove(handle);
            HandleRemoved?.Invoke(element.HandleSource, handle);
        }
    }

    private void OnHandleMoved(IHandleSource handleSource, Handle handle, Unit2D position)
    {
        if (_byHandle.TryGetValue(handle, out var oldPosition))
        {
            var entry = new HandleMapEntry(handleSource, handle);

            if (_byPosition.Move(oldPosition, position, entry))
            {
                _byHandle[handle] = position;

                HandleMoved?.Invoke(handleSource, handle, position);
            }
        }
    }
}

