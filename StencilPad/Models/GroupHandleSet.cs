namespace StencilPad.Models;

public class GroupHandleSet : IHandleSet
{
    public event Action? HandlesChanged;
    public event Action? SelectionChanged;

    public IEnumerable<Handle> Handles => _children.SelectMany(child => child.Handles);

    private readonly List<IHandleSet> _children;
    private readonly List<Handle> _selection;
    
    public GroupHandleSet(IEnumerable<IHandleSet> children)
    {
        _children = new(children);
        _selection = [];
        
        foreach (var child in _children)
        {
            child.HandlesChanged += InvokeHandlesChanged;
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

    private void InvokeHandlesChanged()
    {
        HandlesChanged?.Invoke();
    }
}
