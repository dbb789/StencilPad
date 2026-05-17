using StencilPad.Spatial;

namespace StencilPad.Models;

public class EditablePolygon : Polygon, IHandleSource
{
    private HandleSourceId _id = HandleFactory.NewId();
    private HandleSet _handles;
    private HandleSet _selection;
    private List<int> _selectedEdges;
    private List<int> _selectedVertices;

    public event Action<IHandleSource, Handle, Unit2D, bool>? HandleAdded;
    public event Action<IHandleSource, Handle>? HandleRemoved;
    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    public event Action<IHandleSource, Handle, bool>? HandleSelectionChanged;

    public EditablePolygon()
    {
        _handles = new(4);
        _selection = new(4);
        _selectedEdges = new();
        _selectedVertices = new();
        
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

        UpdateSelectedIndices();
        RebuildHandles();
        ReapplySelection();
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

        UpdateSelectedIndices();
        RebuildHandles();
        ReapplySelection();
    }
    
    protected override void OnCycledVertices(int index)
    {
        base.OnCycledVertices(index);
        
        CycleSelection((Vertices.Count - 1) - index, Vertices.Count);
    }

    public IEnumerable<int> GetSelectedVertices()
    {
        return _selectedVertices;
    }

    public IEnumerable<int> GetSelectedEdges()
    {
        return _selectedEdges;
    }
    
    private void CalculateSelectedVertices(List<int> indices)
    {
        indices.AddRange(_selection.Where(x => x.Key.Polygon.Type == PolygonHandleType.Vertex)
                         .Select(x => x.Key.Polygon.Index));
    }

    private void CalculateSelectedEdges(List<int> indices)
    {
        for (int i = 0; i < Edges.Count; i++)
        {
            if (_selection.Contains(Handle.Move(_id, PolygonHandleKey.Vertex(i))) &&
                _selection.Contains(Handle.Move(_id, PolygonHandleKey.Vertex((i + 1) % Vertices.Count))))
            {
                indices.Add(i);
            }
        }
    }
    
    public void QueryHandles(Action<Handle, Unit2D, bool> func)
    {
        foreach (var handle in _handles)
        {
            func(handle, GetPoint(handle), _selection.Contains(handle));
        }
    }

    public void SetHandleSelected(Handle handle, bool selected)
    {
        if (selected)
        {
            if (_selection.Add(handle))
            {
                UpdateSelectedIndices();
                HandleSelectionChanged?.Invoke(this, handle, true);
            }
        }
        else
        {
            if (_selection.Remove(handle))
            {
                UpdateSelectedIndices();
                HandleSelectionChanged?.Invoke(this, handle, false);
            }
        }
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

    private void VertexReassigned(int index, Vertex prev, Vertex next)
    {
        if (prev.Position != next.Position)
        {
            HandleMoved?.Invoke(this, Handle.Move(_id, PolygonHandleKey.Vertex(index)), next.Position);
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
                HandleMoved?.Invoke(this, Handle.Adjust(_id, PolygonHandleKey.ControlBegin(index)),
                    Vertices[index].Position + next.ControlBeginOffset);

                HandleMoved?.Invoke(this, Handle.Adjust(_id, PolygonHandleKey.ControlEnd((index - 1 + Edges.Count) % Edges.Count)),
                    Vertices.At(index).Position - next.ControlBeginOffset);
            }

            if (prev.ControlEndOffset != next.ControlEndOffset)
            {
                HandleMoved?.Invoke(this, Handle.Adjust(_id, PolygonHandleKey.ControlEnd(index)),
                    Vertices.At(index + 1).Position + next.ControlEndOffset);

                HandleMoved?.Invoke(this, Handle.Adjust(_id, PolygonHandleKey.ControlBegin((index + 1) % Edges.Count)),
                    Vertices.At(index + 1).Position - next.ControlEndOffset);
            }
        }
    }
    
    public void AssignFrom(Polygon other)
    {
        base.AssignFromPolygon(other);

        RebuildHandles();
        UpdateSelectedIndices();
        ReapplySelection();
    }

    public void AssignFrom(EditablePolygon other)
    {
        base.AssignFromPolygon(other);
        
        _id = other._id;

        foreach (var handle in _handles)
        {
            HandleRemoved?.Invoke(this, handle);
        }
        
        _handles.Clear();
        _handles.AddRange(other._handles);
        
        _selection.Clear();
        _selection.AddRange(other._selection);

        foreach (var handle in _handles)
        {
            HandleAdded?.Invoke(this, handle, GetPoint(handle), other._selection.Contains(handle));
        }

        UpdateSelectedIndices();
        ReapplySelection();
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

        UpdateSelectedIndices();
        ReapplySelection();
    }

    private void RebuildHandles()
    {
        // FIXME: This needs to be optimized to avoid unnecessary handle
        // removals and additions when only a few vertices or edges are changed.
        
        foreach (var handle in _handles)
        {
            HandleRemoved?.Invoke(this, handle);
        }
        
        _handles.Clear();

        for (int i = 0; i < Vertices.Count; i++)
        {
            AddHandle(Handle.Move(_id, PolygonHandleKey.Vertex(i)));
        }

        for (int i = 0; i < Edges.Count; i++)
        {
            if (Edges[i].Type == EdgeType.Bezier)
            {
                AddHandle(Handle.Adjust(_id, PolygonHandleKey.ControlBegin(i)));
                AddHandle(Handle.Adjust(_id, PolygonHandleKey.ControlEnd(i)));
            }
        }
    }

    private void AddHandle(Handle handle)
    {
        _handles.Add(handle);
        HandleAdded?.Invoke(this, handle, GetPoint(handle), _selection.Contains(handle));
    }
    
    private void UpdateSelectedIndices()
    {
        _selectedVertices.Clear();
        CalculateSelectedVertices(_selectedVertices);

        _selectedEdges.Clear();
        CalculateSelectedEdges(_selectedEdges);
    }

    private void ReapplySelection()
    {
        foreach (var handle in _handles)
        {
            HandleSelectionChanged?.Invoke(this, handle, _selection.Contains(handle));
        }
    }
}
