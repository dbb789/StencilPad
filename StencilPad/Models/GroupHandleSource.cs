using StencilPad.Spatial;

namespace StencilPad.Models;

public class GroupHandleSource : IHandleSource
{
    private Unit2D _position;
    
    public Unit2D Position
    {
        get => _position;
        set
        {
            if (_position == value)
            {
                return;
            }

            _position = value;

            for (int i = 0; i < _handles.Count; i++)
            {
                var handle = _handles[i];
                
                HandleMoved?.Invoke(this, handle, GetPoint(handle));
            }
        }
    }

    public event Action<IHandleSource, Handle, Unit2D>? HandleAdded;
    public event Action<IHandleSource, Handle>? HandleRemoved;
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    
    public event Action? SelectionChanged;

    public HandleSet Handles => _handles;
    
    private readonly List<IHandleSource> _children;
    private readonly MutableHandleSet _handles;
    private readonly MutableHandleSet _selection;
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

    public HandleSet GetSelectedHandles()
    {
        return _selection;
    }

    public void SetSelectedHandles(HandleSet handles)
    {
        _selection.Clear();
        _selection.AddRange(handles);

        foreach (var child in _children)
        {
            child.SetSelectedHandles([]);
        }

        foreach (var group in _selection.GroupBy(h => _routing[h.HandleSetId]))
        {
            var subSelection = group.Select(h => h).ToList();
            var subHandleSet = new MutableHandleSet(subSelection.Count());

            subHandleSet.AddRange(subSelection);
            
            group.Key.SetSelectedHandles(subHandleSet);
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
    
    private void OnHandleAdded(IHandleSource handleSource, Handle handle, Unit2D position)
    {
        _handles.Add(handle);
        _routing[handle.HandleSetId] = handleSource;        
        HandleAdded?.Invoke(this, handle, position + Position);
    }

    private void OnHandleRemoved(IHandleSource handleSource, Handle handle)
    {
        _handles.Remove(handle);
        HandleRemoved?.Invoke(this, handle);
    }
    
    private void OnHandleMoved(IHandleSource handleSource, Handle handle, Unit2D position)
    {
        HandleMoved?.Invoke(this, handle, position + Position);
    }
}
