using StencilPad.Spatial;

namespace StencilPad.Models;

public class StartEndHandleSource : IHandleSource
{
    public event Action<IHandleSource, Handle, Unit2D, bool>? HandleAdded { add { } remove { } }
    public event Action<IHandleSource, Handle>? HandleRemoved { add { } remove { } }
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    public event Action<IHandleSource, Handle, bool>? HandleSelectionChanged;

    private MutableHandleSet _handles;
    private Unit2D _start;
    private Unit2D _end;
    private MutableHandleSet _selection;
    private HandleSourceId _id = HandleFactory.NewId();

    public Unit2D Start
    {
        get => _start;
        set
        {
            _start = value;
            HandleMoved?.Invoke(this, _handles[0], _start);
        }
    }

    public Unit2D End
    {
        get => _end;
        set
        {
            _end = value;
            HandleMoved?.Invoke(this, _handles[1], _end);
        }
    }

    public StartEndHandleSource(Unit2D start, Unit2D end)
    {
        _handles = new(2);
        _handles.Add(Handle.Move(_id, new StartEndHandleKey(StartEndHandleKey.EndType.Start)));
        _handles.Add(Handle.Move(_id, new StartEndHandleKey(StartEndHandleKey.EndType.End)));

        _selection = new(2);

        _start = start;
        _end = end;
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
        return handle.Key.StartEnd.Type == StartEndHandleKey.EndType.Start ? _start : _end;
    }

    public void SetPoint(Handle handle, Unit2D position)
    {
        if (handle.Key.StartEnd.Type == StartEndHandleKey.EndType.Start)
        {
            Start = position;
        }
        else
        {
            End = position;
        }
    }

    public void AssignFrom(StartEndHandleSource other)
    {
        _id = other._id;
        _start = other._start;
        _end = other._end;
        _selection.Clear();
        _selection.AddRange(other._selection);

        HandleMoved?.Invoke(this, _handles[0], _start);
        HandleMoved?.Invoke(this, _handles[1], _end);
    }

    public StartEndHandleSource DeepClone()
    {
        var clone = new StartEndHandleSource(_start, _end);

        clone._id = _id;
        clone._selection.Clear();
        clone._selection.AddRange(_selection);

        return clone;
    }
}
