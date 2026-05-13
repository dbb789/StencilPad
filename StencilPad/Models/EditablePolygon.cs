using StencilPad.Spatial;

namespace StencilPad.Models;

public class EditablePolygon : Polygon, IHandleSet
{
    public IEnumerable<Handle> Handles => _handles;
    
    private HandleSetId _id = HandleFactory.NewId();
    private List<Handle> _handles;
    private List<Handle> _selection;

    public event Action<Handle, Unit2D>? HandleMoved;
    public event Action? HandlesChanged;
    public event Action? SelectionChanged;

    public EditablePolygon()
    {
        _handles = new();
        _selection = new();

        VertexAdded += OnVertexAdded;
        VertexRemoved += OnVertexRemoved;
        Vertices.ItemReassigned += VertexReassigned;
        Edges.ItemReassigned += EdgeReassigned;
    }

    private void OnVertexAdded(int index)
    {
        for (int i = 0; i < _selection.Count; ++i)
        {
            var handle = _selection[i];
            var key = handle.Key.Polygon;
            
            if (key.Index >= index)
            {
                _selection[i] = Handle.Move(_id, PolygonHandleKey.Vertex(key.Index + 1));
            }
        }

        RebuildHandles();
        SelectionChanged?.Invoke();
    }

    private void OnVertexRemoved(int index)
    {
        for (int i = _selection.Count - 1; i >= 0; --i)
        {
            var handle = _selection[i];
            var key = handle.Key.Polygon;

            if (key.Index == index)
            {
                _selection.RemoveAt(i);
            }
            else if (key.Index > index)
            {
                _selection[i] = Handle.Move(_id, PolygonHandleKey.Vertex(key.Index - 1));
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
        return _selection.Where(x => x.Key.Polygon.Type == PolygonHandleType.Vertex)
            .Select(x => x.Key.Polygon.Index);
    }

    public IEnumerable<int> GetSelectedEdges()
    {
        var edges = new List<int>(Edges.Count);

        GetSelectedEdges(edges);
        
        return edges;
    }

    public void GetSelectedEdges(List<int> edges)
    {
        for (int i = 0; i < Edges.Count; i++)
        {
            if (_selection.Contains(Handle.Move(_id, PolygonHandleKey.Vertex(i))) &&
                _selection.Contains(Handle.Move(_id, PolygonHandleKey.Vertex((i + 1) % Vertices.Count))))
            {
                edges.Add(i);
            }
        }
    }
    
    public void ClearSelection()
    {
        _selection.Clear();
    }

    public Unit2D GetPoint(Handle handle)
    {
        var key = handle.Key.Polygon;

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
        var key = handle.Key.Polygon;

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

    private void VertexReassigned(int index, Vertex prev, Vertex next)
    {
        if (prev.Position != next.Position)
        {
            HandleMoved?.Invoke(Handle.Move(_id, PolygonHandleKey.Vertex(index)), next.Position);
        }
    }

    private void EdgeReassigned(int index, Edge prev, Edge next)
    {
        if (prev.Type != next.Type)
        {
            RebuildHandles();
        }
        else if (next.Type == EdgeType.Bezier)
        {
            if (prev.ControlBeginOffset != next.ControlBeginOffset)
            {
                HandleMoved?.Invoke(Handle.Adjust(_id, PolygonHandleKey.ControlBegin(index)),
                    Vertices[index].Position + next.ControlBeginOffset);

                HandleMoved?.Invoke(Handle.Adjust(_id, PolygonHandleKey.ControlEnd((index - 1 + Edges.Count) % Edges.Count)),
                    Vertices.At(index).Position - next.ControlBeginOffset);
            }

            if (prev.ControlEndOffset != next.ControlEndOffset)
            {
                HandleMoved?.Invoke(Handle.Adjust(_id, PolygonHandleKey.ControlEnd(index)),
                    Vertices.At(index + 1).Position + next.ControlEndOffset);

                HandleMoved?.Invoke(Handle.Adjust(_id, PolygonHandleKey.ControlBegin((index + 1) % Edges.Count)),
                    Vertices.At(index + 1).Position - next.ControlEndOffset);
            }
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
        
        _id = other._id;

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
            var key = handle.Key.Polygon;
            int newIndex = (key.Index - delta + vertexCount) % vertexCount;

            _selection[i] = handle with { Key = new HandleKey(handle.Key.Polygon with { Index = newIndex }) };
        }
        
        SelectionChanged?.Invoke();
    }

    private void RebuildHandles()
    {
        _handles.Clear();

        for (int i = 0; i < Vertices.Count; i++)
        {
            _handles.Add(Handle.Move(_id, PolygonHandleKey.Vertex(i)));
        }

        for (int i = 0; i < Edges.Count; i++)
        {
            if (Edges[i].Type == EdgeType.Bezier)
            {
                _handles.Add(Handle.Adjust(_id, PolygonHandleKey.ControlBegin(i)));
                _handles.Add(Handle.Adjust(_id, PolygonHandleKey.ControlEnd(i)));
            }
        }

        HandlesChanged?.Invoke();
    }
}
