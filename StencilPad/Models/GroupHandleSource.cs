using StencilPad.Spatial;

namespace StencilPad.Models;

public class GroupHandleSource<TChild> : IHandleSource where TChild : IHandleSource<TChild>
{
    public event Action<IHandleSource, Handle, Unit2D, bool>? HandleAdded;
    public event Action<IHandleSource, Handle>? HandleRemoved;
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    public event Action<IHandleSource, Handle, bool>? HandleSelectionChanged;

    private readonly List<TChild> _children;
    private readonly Dictionary<HandleSourceId, TChild> _routing;

    public GroupHandleSource()
    {
        _children = [];
        _routing = [];
    }

    public GroupHandleSource(IEnumerable<TChild> children)
    {
        _children = [];
        _routing = [];

        SetChildren(children);
    }

    public void SetChildren(IEnumerable<TChild> children)
    {
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            Remove(_children[i]);
        }

        _routing.Clear();
        
        foreach (var child in children)
        {
            Add(child);
        }
    }

    public void Add(TChild child)
    {
        _children.Add(child);
        
        child.HandleAdded += OnHandleAdded;
        child.HandleRemoved += OnHandleRemoved;
        child.HandleMoved += OnHandleMoved;
        child.HandleSelectionChanged += OnHandleSelectionChanged;
        
        child.QueryHandles((handle, position, selected) =>
        {
            _routing[handle.HandleSetId] = child;
            HandleAdded?.Invoke(this, handle, position, selected);
        });
    }

    public void Remove(TChild child)
    {
        _children.Remove(child);
        
        child.HandleAdded -= OnHandleAdded;
        child.HandleRemoved -= OnHandleRemoved;
        child.HandleMoved -= OnHandleMoved;
        child.HandleSelectionChanged -= OnHandleSelectionChanged;

        child.QueryHandles((handle, position, selected) =>
        {
            HandleRemoved?.Invoke(this, handle);
        });
    }

    public void QueryHandles(Action<Handle, Unit2D, bool> func)
    {
        foreach (var child in _children)
        {
            child.QueryHandles(func);
        }
    }

    public void SetHandleSelected(Handle handle, bool selected)
    {
        _routing[handle.HandleSetId].SetHandleSelected(handle, selected);
    }

    public void SetPoint(Handle handle, Unit2D position)
    {
        var child = _routing[handle.HandleSetId];
        
        child.SetPoint(handle, position);
    }

    public Unit2D GetPoint(Handle handle)
    {
        var child = _routing[handle.HandleSetId];
        
        return child.GetPoint(handle);
    }

    private void OnHandleAdded(TChild child, Handle handle, Unit2D position, bool selected)
    {
        _routing[handle.HandleSetId] = child;
        HandleAdded?.Invoke(this, handle, position, selected);
    }

    private void OnHandleRemoved(TChild child, Handle handle)
    {
        HandleRemoved?.Invoke(this, handle);
    }
    
    private void OnHandleMoved(TChild child, Handle handle, Unit2D position)
    {
        HandleMoved?.Invoke(this, handle, position);
    }

    private void OnHandleSelectionChanged(TChild child, Handle handle, bool selected)
    {
        HandleSelectionChanged?.Invoke(this, handle, selected);
    }
}

