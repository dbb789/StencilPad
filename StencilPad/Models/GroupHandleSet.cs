using StencilPad.Spatial;

namespace StencilPad.Models;

public class GroupHandleSet : IHandleSet
{
    private readonly record struct GroupHandleKey : IHandleKey
    {
        public IHandleKey ChildKey { get; init; }
        public IHandleSet Child { get; init; }

        public GroupHandleKey(IHandleSet child, IHandleKey childKey)
        {
            Child = child;
            ChildKey = childKey;
        }
    }
    
    public event Action? HandlesChanged;
    public event Action? SelectionChanged;

    public IEnumerable<Handle> Handles => _handles;
    
    private readonly List<IHandleSet> _children;
    private readonly List<Handle> _handles;
    private readonly List<Handle> _selection;
    
    public GroupHandleSet(IEnumerable<IHandleSet> children)
    {
        _children = new(children);
        _handles = [];
        _selection = [];
        
        foreach (var child in _children)
        {
            child.HandlesChanged += ChildHandlesChanged;
        }

        ChildHandlesChanged();
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

    public void SetPoint(Handle handle, Unit2D position)
    {
        var key = handle.Key<GroupHandleKey>();

        key.Child.SetPoint(new Handle(key.ChildKey, handle.Type), position);
    }

    public Unit2D GetPoint(Handle handle)
    {
        var key = handle.Key<GroupHandleKey>();

        return key.Child.GetPoint(new Handle(key.ChildKey, handle.Type));
    }
    
    private void ChildHandlesChanged()
    {
        _handles.Clear();

        foreach (var child in _children)
        {
            foreach (var handle in child.Handles)
            {
                _handles.Add(new Handle(new GroupHandleKey(child, handle.Key<IHandleKey>()), handle.Type));
            }
        }
        
        HandlesChanged?.Invoke();
    }
}
