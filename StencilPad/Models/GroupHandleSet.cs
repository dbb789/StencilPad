using StencilPad.Spatial;

namespace StencilPad.Models;

public class GroupHandleSet : IHandleSet
{
    public Unit2D Position;

    public event Action<Handle, Unit2D>? HandleMoved;
    public event Action? HandlesChanged;
    public event Action? SelectionChanged;

    public IEnumerable<Handle> Handles => _handles;
    
    private readonly List<IHandleSet> _children;
    private readonly List<Handle> _handles;
    private readonly List<Handle> _selection;
    private readonly Dictionary<HandleSetId, IHandleSet> _routing;

    public GroupHandleSet()
    {
        _children = [];
        _handles = [];
        _selection = [];
        _routing = [];
    }

    public GroupHandleSet(IEnumerable<IHandleSet> children)
    {
        _children = [];
        _handles = [];
        _selection = [];
        _routing = [];

        SetChildren(children);
    }

    public void SetChildren(IEnumerable<IHandleSet> children)
    {
        foreach (var child in _children)
        {
            child.HandlesChanged -= RebuildChildHandles;
            child.HandleMoved -= InvokeHandleMoved;
        }
        
        _children.Clear();
        _selection.Clear();
        SelectionChanged?.Invoke();
        
        _children.AddRange(children);
        
        foreach (var child in _children)
        {
            child.HandlesChanged += RebuildChildHandles;
            child.HandleMoved += InvokeHandleMoved;
        }

        RebuildChildHandles();
    }

    public void Add(IHandleSet child)
    {
        _children.Add(child);
        child.HandlesChanged += RebuildChildHandles;
        child.HandleMoved += InvokeHandleMoved;
        
        foreach (var handle in child.Handles)
        {
            _handles.Add(handle);
            _routing[handle.HandleSetId] = child;
        }
        
        HandlesChanged?.Invoke();
    }

    public IEnumerable<Handle> GetSelectedHandles()
    {
        return _selection;
    }

    public void SetSelectedHandles(IEnumerable<Handle> handles)
    {
        _selection.Clear();
        _selection.AddRange(handles);

        foreach (var child in _children)
        {
            child.SetSelectedHandles([]);
        }

        foreach (var group in _selection.GroupBy(h => _routing[h.HandleSetId]))
        {
            group.Key.SetSelectedHandles(group);
        }
        
        SelectionChanged?.Invoke();
    }

    public void SetPoint(Handle handle, Unit2D position)
    {
        _routing[handle.HandleSetId].SetPoint(handle, position - Position);
    }

    public Unit2D GetPoint(Handle handle)
    {
        return _routing[handle.HandleSetId].GetPoint(handle) + Position;
    }
    
    private void RebuildChildHandles()
    {
        _handles.Clear();
        _routing.Clear();

        foreach (var child in _children)
        {
            foreach (var handle in child.Handles)
            {
                _handles.Add(handle);
                _routing[handle.HandleSetId] = child;
            }
        }
        
        HandlesChanged?.Invoke();
    }

    private void InvokeHandleMoved(Handle handle, Unit2D position)
    {
        HandleMoved?.Invoke(handle, position + Position);
    }
}
