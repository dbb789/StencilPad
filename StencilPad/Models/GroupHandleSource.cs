using StencilPad.Spatial;

namespace StencilPad.Models;

public class GroupHandleSource : IHandleSource
{
    public Unit2D Position;

    public event Action<IHandleSource, Handle, Unit2D>? HandleAdded;
    public event Action<IHandleSource, Handle>? HandleRemoved;
    public event Action<Handle, Unit2D>? HandleMoved;
    
    public event Action? SelectionChanged;

    public IEnumerable<Handle> Handles => _handles;
    
    private readonly List<IHandleSource> _children;
    private readonly List<Handle> _handles;
    private readonly List<Handle> _selection;
    private readonly Dictionary<HandleSourceId, IHandleSource> _routing;

    public GroupHandleSource()
    {
        _children = [];
        _handles = [];
        _selection = [];
        _routing = [];
    }

    public GroupHandleSource(IEnumerable<IHandleSource> children)
    {
        _children = [];
        _handles = [];
        _selection = [];
        _routing = [];

        SetChildren(children);
    }

    public void SetChildren(IEnumerable<IHandleSource> children)
    {
        foreach (var child in _children.ToList())
        {
            Remove(child);
        }

        _routing.Clear();
        _selection.Clear();
        SelectionChanged?.Invoke();
        
        foreach (var child in children)
        {
            Add(child);
        }
    }

    public void Add(IHandleSource child)
    {
        _children.Add(child);
        
        child.HandleAdded += OnHandleAdded;
        child.HandleRemoved += OnHandleRemoved;
        child.HandleMoved += OnHandleMoved;
        
        foreach (var handle in child.Handles)
        {
            OnHandleAdded(child, handle, child.GetPoint(handle));
        }
    }

    public void Remove(IHandleSource child)
    {
        _children.Remove(child);
        
        child.HandleAdded -= OnHandleAdded;
        child.HandleRemoved -= OnHandleRemoved;
        child.HandleMoved -= OnHandleMoved;
        
        foreach (var handle in child.Handles)
        {
            OnHandleRemoved(child, handle);
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
    
    private void OnHandleAdded(IHandleSource handleSet, Handle handle, Unit2D position)
    {
        _handles.Add(handle);
        _routing[handle.HandleSetId] = handleSet;        
        HandleAdded?.Invoke(this, handle, position + Position);
    }

    private void OnHandleRemoved(IHandleSource handleSet, Handle handle)
    {
        _handles.Remove(handle);
        HandleRemoved?.Invoke(this, handle);
    }
    
    private void OnHandleMoved(Handle handle, Unit2D position)
    {
        HandleMoved?.Invoke(handle, position + Position);
    }
}
