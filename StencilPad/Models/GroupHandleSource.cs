using StencilPad.Spatial;

namespace StencilPad.Models;

public class GroupHandleSource : IHandleSource
{
    public event Action<IHandleSource, Handle, Unit2D, bool>? HandleAdded;
    public event Action<IHandleSource, Handle>? HandleRemoved;
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    public event Action<IHandleSource, Handle, bool>? HandleSelectionChanged;

    private readonly List<IHandleSource> _children;
    private readonly Dictionary<HandleSourceId, IHandleSource> _routing;

    public GroupHandleSource()
    {
        _children = [];
        _routing = [];
    }

    public GroupHandleSource(IEnumerable<IHandleSource> children)
    {
        _children = [];
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
        child.HandleSelectionChanged += OnHandleSelectionChanged;
        
        child.QueryHandles((handle, position, selected) =>
        {
            _routing[handle.HandleSetId] = child;
            HandleAdded?.Invoke(this, handle, position, selected);
        });
    }

    public void Remove(IHandleSource child)
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
        _routing[handle.HandleSetId].SetPoint(handle, position);
    }

    public Unit2D GetPoint(Handle handle)
    {
        return _routing[handle.HandleSetId].GetPoint(handle);
    }

    private void OnHandleAdded(IHandleSource handleSource, Handle handle, Unit2D position, bool selected)
    {
        _routing[handle.HandleSetId] = handleSource;        
        HandleAdded?.Invoke(this, handle, position, selected);
    }

    private void OnHandleRemoved(IHandleSource handleSource, Handle handle)
    {
        HandleRemoved?.Invoke(this, handle);
    }
    
    private void OnHandleMoved(IHandleSource handleSource, Handle handle, Unit2D position)
    {
        HandleMoved?.Invoke(this, handle, position);
    }

    private void OnHandleSelectionChanged(IHandleSource handleSource, Handle handle, bool selected)
    {
        HandleSelectionChanged?.Invoke(this, handle, selected);
    }
}
