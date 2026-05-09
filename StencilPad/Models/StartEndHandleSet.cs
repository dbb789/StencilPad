using StencilPad.Spatial;

namespace StencilPad.Models;

public class StartEndHandleSet : IHandleSet
{
    public event Action? HandlesChanged;
    public event Action? SelectionChanged;

    public IEnumerable<Handle> Handles { get; } = [ Handle.Bounds(0), Handle.Bounds(1) ];

    private Unit2D _start;
    private Unit2D _end;
    private List<Handle> _selection = [];

    public Unit2D Start
    {
        get => _start;
        set { _start = value; HandlesChanged?.Invoke(); }
    }

    public Unit2D End
    {
        get => _end;
        set { _end = value; HandlesChanged?.Invoke(); }
    }

    public StartEndHandleSet(Unit2D start, Unit2D end)
    {
        _start = start;
        _end = end;
    }

    public Unit2D GetPoint(Handle handle)
    {
        return (handle.Index == 0) ? _start : _end;
    }

    public void SetPoint(Handle handle, Unit2D position)
    {
        if (handle.Index == 0)
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

    public StartEndHandleSet DeepClone()
    {
        return new(_start, _end);
    }
}
