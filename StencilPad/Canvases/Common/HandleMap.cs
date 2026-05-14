using System.Collections.Specialized;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public class HandleMap : IUnitSnap
{
    private Sheet? _sheet;
    private Dictionary<Handle, Unit2D> _byHandle;
    private QuadTree<Handle> _byPosition;
    private List<(Handle, Unit2D)> _queryResults;
    
    public HandleMap()
    {
        var pageSize = new Unit2D(Unit.FromMillimeters(1000), Unit.FromMillimeters(1000));

        _sheet = null;
        _byHandle = new();
        _byPosition = new QuadTree<Handle>(UnitBounds.FromCenterSize(Unit2D.Zero, pageSize),
                                           nodeCapacity: 16,
                                           maxDepth: 10);
        _queryResults = [];
    }

    public void SetSheet(Sheet? sheet)
    {
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
        foreach (var handle in element.HandleSet.Handles)
        {
            Add(handle, element.HandleSet.GetPoint(handle));
        }
    }

    private void Remove(ISheetElement element)
    {
        foreach (var handle in element.HandleSet.Handles)
        {
            Remove(handle);
        }
    }

    private void Add(Handle handle, Unit2D position)
    {
        _byHandle[handle] = position;
        _byPosition.Insert(position, handle);
    }

    private void Remove(Handle handle)
    {
        if (_byHandle.TryGetValue(handle, out var position))
        {
            _byPosition.Remove(position, handle);
            _byHandle.Remove(handle);
        }
    }
}

