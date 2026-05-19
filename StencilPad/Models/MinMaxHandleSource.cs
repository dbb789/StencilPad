using StencilPad.Spatial;

namespace StencilPad.Models;

public class MinMaxHandleSource : IHandleSource
{
    public event Action<IHandleSource, Handle, Unit2D, bool>? HandleAdded { add { } remove { } }
    public event Action<IHandleSource, Handle>? HandleRemoved { add { } remove { } }
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    public event Action<IHandleSource, Handle, bool>? HandleSelectionChanged;

    private HandleSet _handles;
    private Unit2D _min;
    private Unit2D _max;
    private HandleSet _selection;
    private HandleSourceId _id = HandleFactory.NewId();

    public Unit2D Min
    {
        get => _min;
        set
        {
            _min = value;
            HandleMoved?.Invoke(this, _handles[0], _min);
        }
    }

    public Unit2D Max
    {
        get => _max;
        set
        {
            _max = value;
            HandleMoved?.Invoke(this, _handles[1], _max);
        }
    }

    public MinMaxHandleSource(Unit2D start, Unit2D end)
    {
        _handles = new(2);
        _handles.Add(Handle.Move(_id, new MinMaxHandleKey(MinMaxHandleKey.HandleType.Min)));
        _handles.Add(Handle.Move(_id, new MinMaxHandleKey(MinMaxHandleKey.HandleType.Max)));

        _selection = new(2);

        _min = start;
        _max = end;
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
        return handle.GetKey<MinMaxHandleKey>().Type == MinMaxHandleKey.HandleType.Min ? _min : _max;
    }

    public void SetPoint(Handle handle, Unit2D position)
    {
        if (handle.GetKey<MinMaxHandleKey>().Type == MinMaxHandleKey.HandleType.Min)
        {
            Min = position;
        }
        else
        {
            Max = position;
        }
    }

    public void AssignFrom(MinMaxHandleSource other)
    {
        _id = other._id;
        _min = other._min;
        _max = other._max;
        _selection.Clear();
        _selection.AddRange(other._selection);

        HandleMoved?.Invoke(this, _handles[0], _min);
        HandleMoved?.Invoke(this, _handles[1], _max);
    }

    public MinMaxHandleSource DeepClone()
    {
        var clone = new MinMaxHandleSource(_min, _max);

        clone._id = _id;
        clone._selection.Clear();
        clone._selection.AddRange(_selection);

        return clone;
    }
}
