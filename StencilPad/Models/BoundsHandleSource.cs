using StencilPad.Spatial;

namespace StencilPad.Models;

public class BoundsHandleSource : IHandleSource
{
    // This component has a fixed number of handles so we can just hardcode the
    // events to do nothing since we won't be adding or removing any handles.
    public event Action<IHandleSource, Handle, Unit2D, bool>? HandleAdded { add { } remove { } }
    public event Action<IHandleSource, Handle>? HandleRemoved { add { } remove { } }
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    public event Action<IHandleSource, Handle, bool>? HandleSelectionChanged;

    private HandleSourceId _id = HandleFactory.NewId();
    private Handle _nw;
    private Handle _ne;
    private Handle _sw;
    private Handle _se;
    
    private HandleSet _handles;
    private HandleSet _selection;

    // We're not storing this as a UnitBounds because intermediate states can
    // sometimes become normalised which can introduce errors.
    private Unit2D _min;
    private Unit2D _max;

    private UnitTransform _transform = UnitTransform.Identity;
    public UnitTransform Transform
    {
        get => _transform;
        set
        {
            if (_transform != value)
            {
                _transform = value;
                UpdateAllHandles();
            }
        }
    }
    
    public UnitBounds Bounds
    {
        get => UnitBounds.FromMinMax(_min, _max);
        set
        {
            AssignBounds(value);
        }
    }

    public BoundsHandleSource(UnitBounds bounds)
    {
        _nw = Handle.Move(_id, new BoundsHandleKey(BoundsHandleKey.HandleType.NW));
        _ne = Handle.Move(_id, new BoundsHandleKey(BoundsHandleKey.HandleType.NE));
        _sw = Handle.Move(_id, new BoundsHandleKey(BoundsHandleKey.HandleType.SW));
        _se = Handle.Move(_id, new BoundsHandleKey(BoundsHandleKey.HandleType.SE));
        
        _handles = [_nw, _ne, _sw, _se];
        _selection = new(4);
        
        _min = bounds.Min;
        _max = bounds.Max;
    }

    public void QueryHandles(Action<Handle, Unit2D, bool> func)
    {
        for (int i = 0; i < _handles.Count; i++)
        {
            var handle = _handles[i];
            var position = GetPoint(handle);
            var selected = _selection.Contains(handle);

            func(handle, position, selected);
        }
    }

    public void SetHandleSelected(Handle handle, bool selected)
    {
        if (selected)
        {
            if (_selection.Add(handle))
            {
                HandleSelectionChanged?.Invoke(this, handle, true);
            }
        }
        else
        {
            if (_selection.Remove(handle))
            {
                HandleSelectionChanged?.Invoke(this, handle, false);
            }
        }
    }

    public Unit2D GetPoint(Handle handle)
    {
        var type = handle.GetKey<BoundsHandleKey>().Type;
        Unit2D localPos;

        switch (type)
        {
        case BoundsHandleKey.HandleType.NW:
            localPos = new Unit2D(_min.X, _min.Y);
            break;
            
        case BoundsHandleKey.HandleType.NE:
            localPos = new Unit2D(_max.X, _min.Y);
            break;
            
        case BoundsHandleKey.HandleType.SW:
            localPos = new Unit2D(_min.X, _max.Y);
            break;
            
        case BoundsHandleKey.HandleType.SE:
            localPos = new Unit2D(_max.X, _max.Y);
            break;
        default:
            throw new InvalidOperationException($"Invalid handle type: {type}");
        }

        return Transform.Apply(localPos);
    }

    public void SetPoint(Handle handle, Unit2D position)
    {
        var type = handle.GetKey<BoundsHandleKey>().Type;
        var localPos = Transform.InverseApply(position);

        switch (type)
        {
        case BoundsHandleKey.HandleType.NW:
            _min = localPos;
            break;
            
        case BoundsHandleKey.HandleType.NE:
            _min = new Unit2D(_min.X, localPos.Y);
            _max = new Unit2D(localPos.X, _max.Y);
            break;
            
        case BoundsHandleKey.HandleType.SW:
            _min = new Unit2D(localPos.X, _min.Y);
            _max = new Unit2D(_max.X, localPos.Y);
            break;
            
        case BoundsHandleKey.HandleType.SE:
            _max = localPos;
            break;
        }

        // We're going to have to update 3 handles here so we might as
        // well just update them all.
        UpdateAllHandles();
    }

    public void AssignFrom(BoundsHandleSource other)
    {
        _id = other._id;
        _min = other._min;
        _max = other._max;
        _transform = other._transform;
        
        _selection.AssignFrom(other._selection);

        UpdateAllHandles();

        foreach (var handle in _handles)
        {
            HandleSelectionChanged?.Invoke(this, handle, _selection.Contains(handle));
        }
    }

    public BoundsHandleSource DeepClone()
    {
        var clone = new BoundsHandleSource(Bounds);

        clone.AssignFrom(this);

        return clone;
    }

    private void AssignBounds(UnitBounds bounds)
    {
        _min = bounds.Min;
        _max = bounds.Max;

        UpdateAllHandles();
    }

    private void UpdateAllHandles()
    {
        foreach (var handle in _handles)
        {
            HandleMoved?.Invoke(this, handle, GetPoint(handle));
        }
    }
}
