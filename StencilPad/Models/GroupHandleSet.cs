using StencilPad.Spatial;

namespace StencilPad.Models;

public class GroupHandleSet : IHandleSet
{
    private readonly record struct GroupHandleKey : IHandleKey
    {
        public int Index { get; init; }
        public IHandleKey ChildKey { get; init; }

        public GroupHandleKey(int index, IHandleKey childKey)
        {
            Index = index;
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
        _children = [];
        _handles = [];
        _selection = [];

        SetChildren(children);
    }

    public void SetChildren(IEnumerable<IHandleSet> children)
    {
        foreach (var child in _children)
        {
            child.HandlesChanged -= ChildHandlesChanged;
        }
        
        _children.Clear();
        _selection.Clear();
        SelectionChanged?.Invoke();
        
        _children.AddRange(children);
        
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

        _children[key.Index].SetPoint(new Handle(key.ChildKey, handle.Type), position);
    }

    public Unit2D GetPoint(Handle handle)
    {
        var key = handle.Key<GroupHandleKey>();

        return _children[key.Index].GetPoint(new Handle(key.ChildKey, handle.Type));
    }
    
    private void ChildHandlesChanged()
    {
        _handles.Clear();

        for (int i = 0; i < _children.Count; ++i)
        {
            var child = _children[i];
            
            foreach (var handle in child.Handles)
            {
                _handles.Add(new Handle(new GroupHandleKey(i, handle.Key<IHandleKey>()), handle.Type));
            }
        }
        
        HandlesChanged?.Invoke();
    }
}
