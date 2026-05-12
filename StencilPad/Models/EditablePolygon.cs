using StencilPad.Spatial;

namespace StencilPad.Models;

public class EditablePolygon : Polygon, IHandleSet
{
    public IEnumerable<Handle> Handles => _handles;
    
    private List<Handle> _handles;
    private List<Handle> _selection;
    
    public event Action? SelectionChanged;
    public event Action? HandlesChanged;

    public EditablePolygon()
    {
        _handles = new();
        _selection = new();

        Edges.ItemReassigned += EdgeReassigned;
    }

    protected override void OnVertexAdded(int index)
    {
        for (int i = 0; i < _selection.Count; ++i)
        {
            var handle = _selection[i];
            var key = handle.Key<PolygonHandleKey>();
            
            if (key.Index >= index)
            {
                _selection[i] = Handle.Move(PolygonHandleKey.Vertex(key.Index + 1));
            }
        }

        RebuildHandles();
        SelectionChanged?.Invoke();
    }

    protected override void OnVertexRemoved(int index)
    {
        for (int i = _selection.Count - 1; i >= 0; --i)
        {
            var handle = _selection[i];
            var key = handle.Key<PolygonHandleKey>();

            if (key.Index == index)
            {
                _selection.RemoveAt(i);
            }
            else if (key.Index > index)
            {
                _selection[i] = Handle.Move(PolygonHandleKey.Vertex(key.Index - 1));
            }
        }

        RebuildHandles();
        SelectionChanged?.Invoke();
    }
    
    protected override void OnCycledVertices(int index)
    {
        base.OnCycledVertices(index);
        
        CycleSelection((Vertices.Count - 1) - index, Vertices.Count);
    }

    public IEnumerable<int> GetSelectedVertices()
    {
        return _selection.Where(x => x.Key<PolygonHandleKey>().Type == PolygonHandleType.Vertex)
            .Select(x => x.Key<PolygonHandleKey>().Index);
    }

    public IEnumerable<int> GetSelectedEdges()
    {
        var edges = new List<int>(Edges.Count);

        for (int i = 0; i < Edges.Count; i++)
        {
            if (_selection.Contains(Handle.Move(PolygonHandleKey.Vertex(i))) &&
                _selection.Contains(Handle.Move(PolygonHandleKey.Vertex((i + 1) % Vertices.Count))))
            {
                edges.Add(i);
            }
        }

        return edges;
    }

    public void ClearSelection()
    {
        _selection.Clear();
    }

    public Unit2D GetPoint(Handle handle)
    {
        var key = handle.Key<PolygonHandleKey>();

        switch (key.Type)
        {
        case PolygonHandleType.Vertex:
            return Vertices[key.Index].Position;
            
        case PolygonHandleType.ControlBegin:
            return Vertices[key.Index].Position + Edges[key.Index].ControlBeginOffset;
            
        case PolygonHandleType.ControlEnd:
            return Vertices.At(key.Index + 1).Position + Edges[key.Index].ControlEndOffset;
        }

        throw new ArgumentOutOfRangeException(nameof(handle));
    }

    public void SetPoint(Handle handle, Unit2D position)
    {
        var key = handle.Key<PolygonHandleKey>();

        switch (key.Type)
        {
        case PolygonHandleType.Vertex:
            Vertices[key.Index] = Vertices[key.Index] with { Position = position };
            break;
            
        case PolygonHandleType.ControlBegin:
            Edges[key.Index] = Edges[key.Index] with
                { ControlBeginOffset = position - Vertices[key.Index].Position };
            break;
            
        case PolygonHandleType.ControlEnd:
            Edges[key.Index] = Edges[key.Index] with
                { ControlEndOffset = position - Vertices.At(key.Index + 1).Position };
            break;

        default:
            throw new ArgumentOutOfRangeException(nameof(handle));
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
        
        SelectionChanged?.Invoke();
    }

    protected override void OnPolygonChanged()
    {
        HandlesChanged?.Invoke();
    }

    private void EdgeReassigned(int index, Edge prev, Edge next)
    {
        if (prev.Type != next.Type)
        {
            RebuildHandles();
        }
    }
    
    public void AssignFrom(Polygon other)
    {
        base.AssignFromPolygon(other);

        RebuildHandles();
    }

    public void AssignFrom(EditablePolygon other)
    {
        base.AssignFromPolygon(other);

        _handles.Clear();
        _handles.AddRange(other._handles);
        
        _selection.Clear();
        _selection.AddRange(other._selection);

        SelectionChanged?.Invoke();
        HandlesChanged?.Invoke();
    }

    public new EditablePolygon DeepClone()
    {
        var editablePolygon = new EditablePolygon();

        editablePolygon.AssignFrom(this);
        
        return editablePolygon;
    }
    
    private void CycleSelection(int delta, int vertexCount)
    {
        for (int i = 0; i < _selection.Count; ++i)
        {
            var handle = _selection[i];
            var key = handle.Key<PolygonHandleKey>();
            
            _selection[i] = new Handle(new PolygonHandleKey(key.Type, (key.Index - delta + vertexCount) % vertexCount),
                                       handle.Type);
        }
        
        SelectionChanged?.Invoke();
    }

    private void RebuildHandles()
    {
        _handles.Clear();

        for (int i = 0; i < Vertices.Count; i++)
        {
            _handles.Add(Handle.Move(PolygonHandleKey.Vertex(i)));
        }

        for (int i = 0; i < Edges.Count; i++)
        {
            if (Edges[i].Type == EdgeType.Bezier)
            {
                _handles.Add(Handle.Adjust(PolygonHandleKey.ControlBegin(i)));
                _handles.Add(Handle.Adjust(PolygonHandleKey.ControlEnd(i)));
            }
        }

        HandlesChanged?.Invoke();
    }
}
