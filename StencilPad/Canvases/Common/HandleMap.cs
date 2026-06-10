using System.Collections.Specialized;
using System.Diagnostics;
using StencilPad.Common;
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

    public ReadOnlyFlatSet<IHandleMapEntry> SelectedHandles => _selectedHandles;

    private readonly ISettings _settings;
    private readonly Dictionary<Handle, HandleMapEntry> _byHandle;
    private readonly DynamicQuadTree<HandleMapEntry> _byPosition;
    private readonly FlatSet<IHandleMapEntry> _selectedHandles;
    private readonly List<HandleMapEntry> _queryResults;
    
    private Sheet? _sheet;

    public event Action? SheetSelectionChanged;

    public event Action<ISheetElement, Handle, Unit2D>? HandleAdded;
    public event Action<ISheetElement, Handle>? HandleRemoved;
    public event Action<ISheetElement, Handle, Unit2D>? HandleMoved;
    public event Action? HandleSelectionChanged;

    public HandleMap(ISettings settings)
    {
        _settings = settings;
        
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
        _selectedHandles = new(128);
        _queryResults = new(128);
    }

    public void QueryHandles(UnitBounds bounds, List<IHandleMapEntry> results)
    {
        _byPosition.Query(bounds, x => results.Add(x));
    }

    public HandleMapEntry? GetClosestHandle(UnitBounds bounds)
    {
        _queryResults.Clear();
        _byPosition.Query(bounds, x => _queryResults.Add(x));

        HandleMapEntry? closest = null;
        var closestDistance = bounds.Size.Magnitude * 2;

        // Iterate backwards so that in the case of overlap, the most recently
        // added/moved handle is returned.
        for (int i = _queryResults.Count - 1; i >= 0; i--)
        {
            var result = _queryResults[i];
            var distance = (result.Position - bounds.Center).Magnitude;

            if (closest is null || distance < closestDistance)
            {
                closest = result;
                closestDistance = distance;
            }
        }

        return closest;
    }

    public bool TryGetHandleEntry(Handle handle, out IHandleMapEntry entry)
    {
        if (_byHandle.TryGetValue(handle, out var found))
        {
            entry = found;
            return true;
        }

        entry = default!;
        
        return false;
    }
    
    private void SetSheet(Sheet? sheet)
    {
        if (_sheet == sheet)
        {
            return;
        }
        
        if (_sheet is not null)
        {
            foreach (var element in _sheet.Elements)
            {
                Remove(element);
            }

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

    public void SelectAll()
    {
        foreach (var entry in _byHandle.Values)
        {
            entry.SetSelected(entry.Editing);
        }
    }

    public void ClearSelection()
    {
        foreach (var entry in _byHandle.Values)
        {
            entry.SetSelected(false);
        }
    }
    
    public Unit2D? UnitSnap(Unit2D point, IUnitSnapContext context)
    {
        _queryResults.Clear();

        var pointSnapPx = _settings.PointSnapPx;

        // Note that pointSnapPx is a radius, so we need to query a bounds with
        // double the size to ensure we find all potential snaps within the
        // radius.
        var querySize = context.Viewport.FromPixels(pointSnapPx * 2);

        _byPosition.Query(UnitBounds.FromCenterSize(point, new Unit2D(querySize, querySize)),
                          x => _queryResults.Add(x));

        Unit2D? closestSnap = null;
        Unit closestDistance = context.Viewport.FromPixels(pointSnapPx);
        
        foreach (var entry in _queryResults)
        {
            if (!context.CanUnitSnapTo(entry.Element))
            {
                continue;
            }

            if (!context.CanUnitSnapTo(entry.Handle))
            {
                continue;
            }

            var distance = (point - entry.Position).Magnitude;
            
            if (distance < closestDistance)
            {
                closestSnap = entry.Position;
                closestDistance = distance;
            }
        }

        return closestSnap;
    }
    
    private void OnSheetElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (SheetElement element in e.OldItems)
            {
                Remove(element);
            }
        }

        if (e.NewItems is not null)
        {
            foreach (SheetElement element in e.NewItems)
            {
                Add(element);
            }
        }
    }

    private void OnSheetSelectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (SheetElement element in e.OldItems)
            {
                element.QueryHandles((handle, localPosition, selected) =>
                {
                    if (_byHandle.TryGetValue(handle, out var entry))
                    {
                        entry.Editing = false;
                        entry.Selected = false;

                        element.SetHandleSelected(handle, false);
                    }
                    else
                    {
                        Debug.WriteLine($"HandleMap: Failed to clear selection for handle {handle} from element {element}");
                    }
                });
            }
        }

        if (e.NewItems is not null)
        {
            foreach (SheetElement element in e.NewItems)
            {
                element.QueryHandles((handle, localPosition, selected) =>
                {
                    if (_byHandle.TryGetValue(handle, out var entry))
                    {
                        entry.Editing = true;
                    }
                    else
                    {
                        Debug.WriteLine($"HandleMap: Failed to set selection for handle {handle} from element {element}");
                    }
                });
            }
        }
        
        SheetSelectionChanged?.Invoke();
    }
    
    private void Add(ISheetElement element)
    {
        element.QueryHandles((handle, position, selected) =>
        {
            Add(element, handle, position, selected);
        });

        element.HandleAdded += OnHandleAdded;
        element.HandleRemoved += OnHandleRemoved;
        element.HandleMoved += OnHandleMoved;
        element.HandleSelectionChanged += OnHandleSelectionChanged;
    }

    private void Remove(ISheetElement element)
    {
        element.QueryHandles((handle, localPosition, selected) =>
        {
            Remove(element, handle);
        });

        element.HandleAdded -= OnHandleAdded;
        element.HandleRemoved -= OnHandleRemoved;
        element.HandleMoved -= OnHandleMoved;
        element.HandleSelectionChanged -= OnHandleSelectionChanged;
    }

    private void Add(ISheetElement element, Handle handle, Unit2D position, bool selected)
    {
        var entry = new HandleMapEntry
        {
            Element = element,
            Handle = handle,
            Position = position,
            Editing = _sheet?.Selection.Contains(element) ?? false,
            Selected = selected
        };

        if (_byHandle.ContainsKey(handle))
        {
            Debug.WriteLine($"HandleMap: Attempted to add duplicate handle {handle} from element {element}");
            return;
        }
        
        _byHandle[handle] = entry;
        _byPosition.Insert(position, entry);

        if (selected)
        {
            _selectedHandles.Add(entry);
        }

        HandleAdded?.Invoke(element, handle, position);
    }

    private void Remove(ISheetElement element, Handle handle)
    {
        if (_byHandle.TryGetValue(handle, out var entry))
        {
            _byPosition.Remove(entry);
            _byHandle.Remove(handle);

            if (entry.Selected)
            {
                _selectedHandles.Remove(entry);
            }
            
            HandleRemoved?.Invoke(element, handle);
        }
        else
        {
            Debug.WriteLine($"HandleMap: Attempted to remove unknown handle {handle} from element {element}");
        }
    }

    private void OnHandleAdded(ISheetElement element, Handle handle, Unit2D position, bool selected)
    {
        Add(element, handle, position, selected);
    }

    private void OnHandleRemoved(ISheetElement element, Handle handle)
    {
        Remove(element, handle);
    }

    private void OnHandleMoved(ISheetElement element, Handle handle, Unit2D position)
    {
        UpdateHandle(element, handle, position);
    }

    private void OnHandleSelectionChanged(ISheetElement element, Handle handle, bool selected)
    {
        if (_byHandle.TryGetValue(handle, out var entry))
        {
            entry.Selected = selected;

            if (selected)
            {
                _selectedHandles.Add(entry);
            }
            else
            {
                _selectedHandles.Remove(entry);
            }
            
            HandleSelectionChanged?.Invoke();
        }
        else
        {
            Debug.WriteLine($"HandleMap: Received HandleSelectionChanged for unknown handle {handle}");
        }
    }

    private void UpdateHandle(ISheetElement element, Handle handle, Unit2D position)
    {
        if (_byHandle.TryGetValue(handle, out var entry))
        {
            if (_byPosition.Move(position, entry))
            {
                entry.Position = position;
                
                HandleMoved?.Invoke(element, handle, position);
            }
            else
            {
                Debug.WriteLine($"HandleMap: Failed to move handle {handle} from {entry.Position} to new position {position} during transform change");
                
                _byPosition.VisitAllValues((pos, e) =>
                {
                    if (e.Handle == handle)
                    {
                        Debug.WriteLine($"HandleMap: Found handle {handle} at position {pos} during visit");
                    }
                });
            }
        }
        else
        {
            Debug.WriteLine($"HandleMap: Received TransformChanged for unknown handle {handle}");
        }
    }
}

