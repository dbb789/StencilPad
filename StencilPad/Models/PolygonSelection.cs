namespace StencilPad.Models;

public class PolygonSelection
{
    public IEnumerable<Handle> Selection => _selection;

    private List<Handle> _selection;

    public event Action? Changed;

    public PolygonSelection()
    {
        _selection = new(4);
    }

    private PolygonSelection(PolygonSelection other)
    {
        _selection = new(other._selection);
    }
    
    public void AssignFrom(PolygonSelection other)
    {
        _selection = new(other._selection);
        Changed?.Invoke();
    }

    public void AssignFrom(IEnumerable<Handle> selection)
    {
        _selection = new(selection);
        Changed?.Invoke();
    }

    public void Add(Handle selectable)
    {
        if (Contains(selectable))
        {
            return;
        }

        _selection.Add(selectable);
        Changed?.Invoke();
    }

    public void Remove(Handle selectable)
    {
        if (!Contains(selectable))
        {
            return;
        }

        _selection.Remove(selectable);
        Changed?.Invoke();
    }

    public void Clear()
    {
        if (_selection.Count == 0)
        {
            return;
        }

        _selection.Clear();
        Changed?.Invoke();
    }

    public void VertexAdded(int vertexIndex)
    {
        for (int i = 0; i < _selection.Count; ++i)
        {
            var handle = _selection[i];
            var key = handle.Key<PolygonHandleKey>();
            
            if (key.Index >= vertexIndex)
            {
                _selection[i] = Handle.Move(PolygonHandleKey.Vertex(key.Index + 1));
            }
        }

        Changed?.Invoke();
    }

    public void VertexRemoved(int vertexIndex)
    {
        for (int i = _selection.Count - 1; i >= 0; --i)
        {
            var handle = _selection[i];
            var key = handle.Key<PolygonHandleKey>();

            if (key.Index == vertexIndex)
            {
                _selection.RemoveAt(i);
            }
            else if (key.Index > vertexIndex)
            {
                _selection[i] = Handle.Move(PolygonHandleKey.Vertex(key.Index - 1));
            }
        }

        Changed?.Invoke();
    }
    
    public void Cycle(int delta, int vertexCount)
    {
        for (int i = 0; i < _selection.Count; ++i)
        {
            var handle = _selection[i];
            var key = handle.Key<PolygonHandleKey>();
            
            
            _selection[i] = new Handle(new PolygonHandleKey(key.Type, (key.Index - delta + vertexCount) % vertexCount),
                                       handle.Type);
        }
        
        Changed?.Invoke();
    }
    
    public void AssignFromWithoutNotify(PolygonSelection other)
    {
        _selection.Clear();
        _selection.AddRange(other._selection);
    }

    public PolygonSelection DeepClone()
    {
        return new PolygonSelection(this);
    }

    private bool Contains(Handle selectable)
    {
        return _selection.Contains(selectable);
    }
}
