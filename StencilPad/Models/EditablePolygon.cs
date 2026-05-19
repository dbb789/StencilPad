using StencilPad.Spatial;

namespace StencilPad.Models;

public class EditablePolygon : Polygon, IHandleSource
{
    private HandleSourceId _id = HandleFactory.NewId();
    private HandleSet _handles;
    private HandleSet _selection;

    private bool _indicesDirty;
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
        EdgeRemoved += OnEdgeRemoved;
        Vertices.ItemReassigned += VertexReassigned;
        Edges.ItemReassigned += EdgeReassigned;
        ClosedChanged += OnClosedChanged;
    }

    private void OnVertexAdded(int index, ulong key)
    {
        var handle = Handle.Move(_id, PolygonHandleKey.Vertex(key));

        AddHandle(handle);

        _indicesDirty = true;
    }

    private void OnVertexRemoved(int index, ulong key)
    {
        var handle = Handle.Move(_id, PolygonHandleKey.Vertex(key));
        
        _selection.Remove(handle);

        _handles.Remove(handle);
        HandleRemoved?.Invoke(this, handle);
        
        _indicesDirty = true;
    }

    private void OnEdgeRemoved(int index, ulong key)
    {
        var beginHandle = Handle.Adjust(_id, PolygonHandleKey.ControlBegin(key));
        var endHandle = Handle.Adjust(_id, PolygonHandleKey.ControlEnd(key));

        _selection.Remove(beginHandle);
        _selection.Remove(endHandle);

        if (_handles.Remove(beginHandle))
        {
            HandleRemoved?.Invoke(this, beginHandle);
        }

        if (_handles.Remove(endHandle))
        {
            HandleRemoved?.Invoke(this, endHandle);
        }
        
        _indicesDirty = true;
    }

    private void OnClosedChanged(bool closed)
    {
        _indicesDirty = true;
    }

    public List<int> GetSelectedVertices()
    {
        if (_indicesDirty)
        {
            UpdateSelectedIndices();
            _indicesDirty = false;
        }
        
        return _selectedVertices;
    }

    public List<int> GetSelectedEdges()
    {
        if (_indicesDirty)
        {
            UpdateSelectedIndices();
            _indicesDirty = false;
        }
        
        return _selectedEdges;
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
        var key = handle.GetKey<PolygonHandleKey>();

        switch (key.Type)
        {
        case PolygonHandleType.Vertex:
            return Vertices.GetByKey(key.Key).Position;
            
        case PolygonHandleType.ControlBegin:
        {
            var index = Edges.IndexOfKey(key.Key);
            
            return Vertices.At(index).Position + Edges.At(index).ControlBeginOffset;
        }
        
        case PolygonHandleType.ControlEnd:
        {
            var index = Edges.IndexOfKey(key.Key);
            
            return Vertices.At(index + 1).Position + Edges.At(index).ControlEndOffset;
        }
        
        default:
            throw new ArgumentOutOfRangeException(nameof(handle), $"Unexpected handle type: {key.Type}");
        }
    }

    public void SetPoint(Handle handle, Unit2D position)
    {
        var key = handle.GetKey<PolygonHandleKey>();

        switch (key.Type)
        {
        case PolygonHandleType.Vertex:
        {
            var index = Vertices.IndexOfKey(key.Key);
            
            Vertices[index] = Vertices[index] with { Position = position };
            break;
        }
        case PolygonHandleType.ControlBegin:
        {
            var index = Edges.IndexOfKey(key.Key);
            
            Edges[index] = Edges[index] with
                           { ControlBeginOffset = position - Vertices.At(index).Position };
            break;
        }
        case PolygonHandleType.ControlEnd:
        {
            var index = Edges.IndexOfKey(key.Key);
            
            Edges[index] = Edges[index] with
                           { ControlEndOffset = position - Vertices.At(index + 1).Position };
            break;
        }
        default:
            throw new ArgumentOutOfRangeException(nameof(handle));
        }
    }

    private void VertexReassigned(int index, ulong key, Vertex prev, Vertex next)
    {
        if (prev.Position != next.Position)
        {
            HandleMoved?.Invoke(this, Handle.Move(_id, PolygonHandleKey.Vertex(key)), next.Position);

            var prevEdge = Edges.At(index - 1);

            if (prevEdge.Type == EdgeType.Bezier)
            {
                HandleMoved?.Invoke(this, Handle.Adjust(_id, PolygonHandleKey.ControlEnd(Edges.KeyAt(index - 1))),
                                    next.Position + prevEdge.ControlEndOffset);
            }
            
            var edge = Edges.At(index);

            if (edge.Type == EdgeType.Bezier)
            {
                HandleMoved?.Invoke(this, Handle.Adjust(_id, PolygonHandleKey.ControlBegin(Edges.KeyAt(index))),
                                    next.Position + edge.ControlBeginOffset);
            }
        }
    }

    private void EdgeReassigned(int index, ulong key, Edge prev, Edge next)
    {
        if (prev.Type != next.Type)
        {
            var beginHandle = Handle.Adjust(_id, PolygonHandleKey.ControlBegin(key));
            var endHandle = Handle.Adjust(_id, PolygonHandleKey.ControlEnd(key));

            if (prev.Type == EdgeType.Bezier)
            {
                _handles.Remove(beginHandle);
                _handles.Remove(endHandle);
                HandleRemoved?.Invoke(this, beginHandle);
                HandleRemoved?.Invoke(this, endHandle);
            }
            else if (next.Type == EdgeType.Bezier)
            {
                AddHandle(beginHandle);
                AddHandle(endHandle);
            }
        }
        else if (next.Type == EdgeType.Bezier)
        {
            if (prev.ControlBeginOffset != next.ControlBeginOffset)
            {
                HandleMoved?.Invoke(this, Handle.Adjust(_id, PolygonHandleKey.ControlBegin(key)),
                                    Vertices.At(index).Position + next.ControlBeginOffset);
            }

            if (prev.ControlEndOffset != next.ControlEndOffset)
            {
                HandleMoved?.Invoke(this, Handle.Adjust(_id, PolygonHandleKey.ControlEnd(key)),
                                    Vertices.At(index + 1).Position + next.ControlEndOffset);
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
    
    private void RebuildHandles()
    {
        foreach (var handle in _handles)
        {
            HandleRemoved?.Invoke(this, handle);
        }
        
        _handles.Clear();

        for (int i = 0; i < Vertices.Count; i++)
        {
            AddHandle(Handle.Move(_id, PolygonHandleKey.Vertex(Vertices.KeyAt(i))));
        }

        for (int i = 0; i < Edges.Count; i++)
        {
            if (Edges[i].Type == EdgeType.Bezier)
            {
                AddHandle(Handle.Adjust(_id, PolygonHandleKey.ControlBegin(Edges.KeyAt(i))));
                AddHandle(Handle.Adjust(_id, PolygonHandleKey.ControlEnd(Edges.KeyAt(i))));
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

        foreach (var handle in _selection)
        {
            var key = handle.GetKey<PolygonHandleKey>();

            if (key.Type == PolygonHandleType.Vertex)
            {
                _selectedVertices.Add(Vertices.IndexOfKey(key.Key));
            }
        }

        _selectedVertices.Sort();
        _selectedEdges.Clear();

        for (int i = 0; i < _selectedVertices.Count - 1; ++i)
        {
            if (_selectedVertices[i] == _selectedVertices[i + 1] - 1)
            {
                _selectedEdges.Add(_selectedVertices[i]);
            }
        }

        if (Closed && _selectedVertices.Count > 1 && _selectedVertices[0] == 0 &&
            _selectedVertices[^1] == Vertices.Count - 1)
        {
            _selectedEdges.Add(Vertices.Count - 1);
        }
    }

    private void ReapplySelection()
    {
        foreach (var handle in _handles)
        {
            HandleSelectionChanged?.Invoke(this, handle, _selection.Contains(handle));
        }
    }
}
