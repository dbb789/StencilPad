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
    private Dictionary<Handle, HandleMapEntry> _byHandle;
    private DynamicQuadTree<HandleMapEntry> _byPosition;
    private List<HandleMapEntry> _queryResults;

    public event Action? SheetSelectionChanged;

    public event Action<IHandleSource, Handle, Unit2D>? HandleAdded;
    public event Action<IHandleSource, Handle>? HandleRemoved;
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    public event Action? HandleSelectionChanged;

    public HandleMap()
    {
        var maxBounds = UnitBounds.FromCenterSize(Unit2D.Zero, SheetFormat.MaxSize);
        var initialBounds = UnitBounds.FromCenterSize(Unit2D.Zero,
                                                      new Unit2D(Unit.FromMillimeters(400),
                                                                 Unit.FromMillimeters(400)));

        _sheet = null;
        _byHandle = new();
        _byPosition = new DynamicQuadTree<HandleMapEntry>(maxBounds,
                                                          initialBounds,
                                                          nodeCapacity: 64,
                                                          maxDepth: 32);
        _queryResults = new(128);
    }

    public void QueryHandles(UnitBounds bounds, List<HandleMapEntry> results)
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
            _sheet.Elements.CollectionChanged -= OnSheetElementsChanged;
            _sheet.Selection.CollectionChanged -= OnSheetSelectionChanged;
        }
        
        _sheet = sheet;
        
        if (_sheet is not null)
        {
            foreach (var element in _sheet.Elements)
            {
                Add(element);
            }
            
            _sheet.Elements.CollectionChanged += OnSheetElementsChanged;
            _sheet.Selection.CollectionChanged += OnSheetSelectionChanged;
        }
    }

    public Unit2D UnitSnap(Unit2D point, Handle? selfHandle)
    {
        _queryResults.Clear();
        _byPosition.Query(UnitBounds.FromCenterSize(point, new Unit2D(Unit.FromMillimeters(5),
                                                                      Unit.FromMillimeters(5))),
                          _queryResults);

        Unit2D? closestSnap = null;
        Unit closestDistance = Unit.FromMillimeters(5);
        
        foreach (var entry in _queryResults)
        {
            if (selfHandle is not null && entry.Handle == selfHandle)
            {
                continue;
            }
            
            var distance = (point - entry.Position).Magnitude;
            
            if (closestSnap is null || distance < closestDistance)
            {
                closestSnap = entry.Position;
                closestDistance = distance;
            }
        }

        return closestSnap ?? point;
    }
    
    private void OnSheetElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

    private void OnSheetSelectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (SheetElement element in e.NewItems)
            {
                foreach (var handle in element.HandleSource.Handles)
                {
                    if (_byHandle.TryGetValue(handle, out var entry))
                    {
                        entry.ElementSelected = true;
                    }
                }
            }
        }

        if (e.OldItems is not null)
        {
            foreach (SheetElement element in e.OldItems)
            {
                foreach (var handle in element.HandleSource.Handles)
                {
                    if (_byHandle.TryGetValue(handle, out var entry))
                    {
                        entry.ElementSelected = false;
                    }
                }
            }
        }
        
        SheetSelectionChanged?.Invoke();
    }

    private void Add(ISheetElement element)
    {
        foreach (var handle in element.HandleSource.Handles)
        {
            Add(element, handle, element.HandleSource.GetPoint(handle));    
        }

        element.HandleSource.HandleMoved += OnHandleMoved;
        element.HandleSource.SelectionChanged += OnHandleSelectionChanged;
    }

    private void Remove(ISheetElement element)
    {
        foreach (var handle in element.HandleSource.Handles)
        {
            Remove(element, handle);
        }
        
        element.HandleSource.HandleMoved -= OnHandleMoved;
        element.HandleSource.SelectionChanged -= OnHandleSelectionChanged;
    }

    private void Add(ISheetElement element, Handle handle, Unit2D position)
    {
        var entry = new HandleMapEntry
        {
            Element = element,
            Handle = handle,
            Position = position,
            ElementSelected = _sheet?.Selection.Contains(element) ?? false,
        };

        _byHandle[handle] = entry;
        _byPosition.Insert(position, entry);
        HandleAdded?.Invoke(element.HandleSource, handle, position);
    }

    private void Remove(ISheetElement element, Handle handle)
    {
        if (_byHandle.TryGetValue(handle, out var entry))
        {
            _byPosition.Remove(entry.Position, entry);
            _byHandle.Remove(handle);
            
            HandleRemoved?.Invoke(element.HandleSource, handle);
        }
    }

    private void OnHandleMoved(IHandleSource handleSource, Handle handle, Unit2D position)
    {
        if (_byHandle.TryGetValue(handle, out var entry))
        {
            if (_byPosition.Move(entry.Position, position, entry))
            {
                entry.Position = position;
                
                HandleMoved?.Invoke(handleSource, handle, position);
            }
        }
    }

    private void OnHandleSelectionChanged(IHandleSource handleSource)
    {
        var selected = handleSource.GetSelectedHandles();
        
        foreach (var handle in handleSource.Handles)
        {
            if (_byHandle.TryGetValue(handle, out var entry))
            {
                entry.HandleSelected = selected.Contains(handle);
            }
        }

        HandleSelectionChanged?.Invoke();
    }
}

