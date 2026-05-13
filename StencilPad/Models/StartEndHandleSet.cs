using StencilPad.Spatial;

namespace StencilPad.Models;

public class StartEndHandleSet : IHandleSet
{
    public event Action<Handle, Unit2D>? HandleMoved;
    public event Action? HandlesChanged { add { } remove { } }
    public event Action? SelectionChanged;

    public IEnumerable<Handle> Handles => [
        Handle.Move(_id, new StartEndHandleKey(StartEndHandleKey.EndType.Start)),
        Handle.Move(_id, new StartEndHandleKey(StartEndHandleKey.EndType.End))
    ];

    private Unit2D _start;
    private Unit2D _end;
    private List<Handle> _selection = [];
    private HandleSetId _id = HandleFactory.NewId();

    public Unit2D Start
    {
        get => _start;
        set
        {
            _start = value;
            HandleMoved?.Invoke(Handles.ElementAt(0), _start);
        }
    }

    public Unit2D End
    {
        get => _end;
        set
        {
            _end = value;
            HandleMoved?.Invoke(Handles.ElementAt(1), _end);
        }
    }

    public StartEndHandleSet(Unit2D start, Unit2D end)
    {
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

    public IEnumerable<Handle> GetSelectedHandles()
    {
        return _selection;
    }

    public void SetSelectedHandles(IEnumerable<Handle> handles)
    {
        _selection.Clear();
        _selection.AddRange(handles);
        
        SelectionChanged?.Invoke();
    }

    public void AssignFrom(StartEndHandleSet other)
    {
        _id = other._id;
        _start = other._start;
        _end = other._end;
        _selection.Clear();
        _selection.AddRange(other._selection);

        HandleMoved?.Invoke(Handles.ElementAt(0), _start);
        HandleMoved?.Invoke(Handles.ElementAt(1), _end);
    }

    public StartEndHandleSet DeepClone()
    {
        var clone = new StartEndHandleSet(_start, _end);

        clone._id = _id;
        clone._selection.Clear();
        clone._selection.AddRange(_selection);

        return clone;
    }
}
