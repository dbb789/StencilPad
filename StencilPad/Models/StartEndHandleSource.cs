using StencilPad.Spatial;

namespace StencilPad.Models;

public class StartEndHandleSource : IHandleSource
{
    public event Action<IHandleSource, Handle, Unit2D>? HandleAdded { add { } remove { } }
    public event Action<IHandleSource, Handle>? HandleRemoved { add { } remove { } }
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    public event Action<IHandleSource>? SelectionChanged;

    public HandleSet Handles => _handles;

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

    public HandleSet GetSelectedHandles()
    {
        return _selection;
    }

    public void SetSelectedHandles(HandleSet handles)
    {
        _selection.Clear();
        _selection.AddRange(handles);
        
        SelectionChanged?.Invoke(this);
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
