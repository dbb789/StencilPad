using StencilPad.Spatial;

namespace StencilPad.Models;

public class GroupHandleSource : IHandleSource
{
    public event Action<IHandleSource, Handle, Unit2D, bool>? HandleAdded;
    public event Action<IHandleSource, Handle>? HandleRemoved;
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    public event Action<IHandleSource, Handle, bool>? HandleSelectionChanged;

    private readonly List<ISheetElement> _children;
    private readonly Dictionary<HandleSourceId, ISheetElement> _routing;

    public GroupHandleSource()
    {
        _children = [];
        _routing = [];
    }

    public GroupHandleSource(IEnumerable<ISheetElement> children)
    {
        _children = [];
        _routing = [];

        SetChildren(children);
    }

    public void SetChildren(IEnumerable<ISheetElement> children)
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

    public void Add(ISheetElement child)
    {
        _children.Add(child);
        
        child.HandleAdded += OnHandleAdded;
        child.HandleRemoved += OnHandleRemoved;
        child.HandleMoved += OnHandleMoved;
        child.HandleSelectionChanged += OnHandleSelectionChanged;
        child.TransformChanged += OnTransformChanged;
        
        child.QueryHandles((handle, localPosition, selected) =>
        {
            _routing[handle.HandleSetId] = child;
            HandleAdded?.Invoke(this, handle, child.Transform.Apply(localPosition), selected);
        });
    }

    public void Remove(ISheetElement child)
    {
        _children.Remove(child);
        
        child.HandleAdded -= OnHandleAdded;
        child.HandleRemoved -= OnHandleRemoved;
        child.HandleMoved -= OnHandleMoved;
        child.HandleSelectionChanged -= OnHandleSelectionChanged;
        child.TransformChanged -= OnTransformChanged;

        child.QueryHandles((handle, localPosition, selected) =>
        {
            HandleRemoved?.Invoke(this, handle);
        });
    }

    public void QueryHandles(Action<Handle, Unit2D, bool> func)
    {
        foreach (var child in _children)
        {
            child.QueryHandles((handle, localPosition, selected) =>
            {
                func(handle, child.Transform.Apply(localPosition), selected);
            });
        }
    }

    public void SetHandleSelected(Handle handle, bool selected)
    {
        _routing[handle.HandleSetId].SetHandleSelected(handle, selected);
    }

    public void SetPoint(Handle handle, Unit2D groupLocalPosition)
    {
        var child = _routing[handle.HandleSetId];
        child.SetPoint(handle, child.Transform.InverseApply(groupLocalPosition));
    }

    public Unit2D GetPoint(Handle handle)
    {
        var child = _routing[handle.HandleSetId];
        return child.Transform.Apply(child.GetPoint(handle));
    }

    private void OnHandleAdded(ISheetElement child, Handle handle, Unit2D localPosition, bool selected)
    {
        _routing[handle.HandleSetId] = child;
        HandleAdded?.Invoke(this, handle, child.Transform.Apply(localPosition), selected);
    }

    private void OnHandleRemoved(ISheetElement child, Handle handle)
    {
        HandleRemoved?.Invoke(this, handle);
    }
    
    private void OnHandleMoved(ISheetElement child, Handle handle, Unit2D localPosition)
    {
        HandleMoved?.Invoke(this, handle, child.Transform.Apply(localPosition));
    }

    private void OnHandleSelectionChanged(ISheetElement child, Handle handle, bool selected)
    {
        HandleSelectionChanged?.Invoke(this, handle, selected);
    }

    private void OnTransformChanged(ISheetElement child)
    {
        child.QueryHandles((handle, localPosition, selected) =>
        {
            HandleMoved?.Invoke(this, handle, child.Transform.Apply(localPosition));
        });
    }
}

