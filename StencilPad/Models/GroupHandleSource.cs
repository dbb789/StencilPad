using System.Diagnostics;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class GroupHandleSource<TChild> : IHandleSource where TChild : IHandleSource<TChild>
{
    public event Action<IHandleSource, Handle, Unit2D, bool>? HandleAdded;
    public event Action<IHandleSource, Handle>? HandleRemoved;
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    public event Action<IHandleSource, Handle, bool>? HandleSelectionChanged;

    private readonly List<TChild> _children;
    private readonly Dictionary<Handle, TChild> _mapping;

    public GroupHandleSource()
    {
        _children = [];
        _mapping = [];
    }

    public GroupHandleSource(IEnumerable<TChild> children)
    {
        _children = [];
        _mapping = [];

        SetChildren(children);
    }

    public void SetChildren(IEnumerable<TChild> children)
    {
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            Remove(_children[i]);
        }

        if (_mapping.Count > 0)
        {
            Debug.WriteLine($"Warning: GroupHandleSource had {_mapping.Count} handles still mapped after clearing children.");
            _mapping.Clear();
        }
        
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
            _mapping[handle] = child;
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
            _mapping.Remove(handle);
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
        if (!TryGetChild(handle, out var child))
        {
            return;
        }

        child.SetHandleSelected(handle, selected);
    }

    public void SetPoint(Handle handle, Unit2D position)
    {
        if (!TryGetChild(handle, out var child))
        {
            return;
        }

        child.SetPoint(handle, position);
    }

    public Unit2D GetPoint(Handle handle)
    {
        if (!TryGetChild(handle, out var child))
        {
            return Unit2D.Zero;
        }

        return child.GetPoint(handle);
    }

    private bool TryGetChild(Handle handle, out TChild child)
    {
        if (!_mapping.TryGetValue(handle, out child!))
        {
            Debug.WriteLine($"Handle {handle} not found in any child.");
            return false;
        }

        return true;
    }
    
    private void OnHandleAdded(TChild child, Handle handle, Unit2D position, bool selected)
    {
        _mapping[handle] = child;
        HandleAdded?.Invoke(this, handle, position, selected);
    }

    private void OnHandleRemoved(TChild child, Handle handle)
    {
        _mapping.Remove(handle);
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

