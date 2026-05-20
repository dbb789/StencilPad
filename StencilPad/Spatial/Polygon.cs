namespace StencilPad.Spatial;

public class Polygon : IPolygon
{
    public IKeyedList<Vertex> Vertices => _vertices;
    public IKeyedList<Edge> Edges => _edges;
    public bool Closed => _closed;

    private readonly KeyedList<Vertex> _vertices;
    private readonly KeyedList<Edge> _edges;
    private bool _closed;

    public event Action<int, ulong>? VertexAdded;
    public event Action<int, ulong>? VertexRemoved;
    public event Action<int, ulong>? EdgeAdded;
    public event Action<int, ulong>? EdgeRemoved;

    // This is the result of a bulk update that has rearranged all or most
    // vertices or edges - we've deliberately avoided invoking
    // Vertices.ItemReassigned or Edges.ItemReassigned.
    public event Action? InvalidateAllPositions;

    // Signals to the renderer that this polygon needs to be rebuilt.
    public event Action? GeometryChanged;
    
    public Polygon()
    {
        _vertices = new(4);
        _edges = new(4);

        _vertices.ItemReassigned += VertexReassigned;
        _edges.ItemReassigned += EdgeReassigned;

        _closed = false;
    }

    public void AddVertex(Vertex vertex)
    {
        if (_closed)
        {
            throw new InvalidOperationException("Cannot append vertex to a closed polygon.");
        }

        InsertVertex(_vertices.Count, vertex);
    }

    public void InsertVertex(int index, Vertex vertex)
    {
        if (index < 0 || index > _vertices.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        int newEdgeIndex = -1;

        _vertices.Insert(index, vertex);

        if (_vertices.Count > 1)
        {
            // Appends a new edge at the end if inserting at the end, otherwise
            // inserts the edge with the same index as the vertex.
            newEdgeIndex = Math.Min(index, _edges.Count);
            _edges.Insert(newEdgeIndex, new Edge());
        }

        VertexAdded?.Invoke(index, _vertices.KeyAt(index));
        
        if (newEdgeIndex >= 0)
        {
            EdgeAdded?.Invoke(newEdgeIndex, _edges.KeyAt(newEdgeIndex));
        }
        
        GeometryChanged?.Invoke();
    }
    
    public void DeleteVertex(int index)
    {
        if (index < 0 || index >= _vertices.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        if (_vertices.Count <= 1)
        {
            throw new InvalidOperationException("Cannot delete vertex from a polygon with 1 or fewer vertices.");
        }

        var vertexKey = _vertices.KeyAt(index);
        
        _vertices.RemoveAt(index);

        var edgeIndex = _closed ? index : Math.Min(index, _edges.Count - 1);
        var edgeKey = _edges.KeyAt(edgeIndex);
        
        _edges.RemoveAt(edgeIndex);

        VertexRemoved?.Invoke(index, vertexKey);
        EdgeRemoved?.Invoke(edgeIndex, edgeKey);

        if (_closed && _vertices.Count < 3)
        {
            _closed = false;

            if (_edges.Count > 0)
            {
                var lastEdgeIndex = _edges.Count - 1;
                var lastEdgeKey = _edges.KeyAt(lastEdgeIndex);
                
                _edges.RemoveAt(lastEdgeIndex);
                EdgeRemoved?.Invoke(lastEdgeIndex, lastEdgeKey);
            }
        }

        GeometryChanged?.Invoke();
    }
    
    public void Open(int index)
    {
        if (!_closed)
        {
            return;
        }

        int offset = (_edges.Count - 1) - index;
        
        _vertices.RotateIndices(-offset);
        _edges.RotateIndices(-offset);

        var edgeIndex = _edges.Count - 1;
        var edgeKey = _edges.KeyAt(edgeIndex);
        
        _edges.RemoveAt(edgeIndex);
        _closed = false;
        
        EdgeRemoved?.Invoke(edgeIndex, edgeKey);
        InvalidateAllPositions?.Invoke();
        GeometryChanged?.Invoke();
    }

    public void Close()
    {
        if (_closed || _vertices.Count <= 2)
        {
            return;
        }
        
        _edges.Add(new Edge());
        _closed = true;

        EdgeAdded?.Invoke(_edges.Count - 1, _edges.KeyAt(_edges.Count - 1));
        GeometryChanged?.Invoke();
    }

    public void SetControlBegin(int edgeIndex, Unit2D position)
    {
        var offset = position - Vertices.At(edgeIndex).Position;

        _edges[edgeIndex] = _edges[edgeIndex] with
            { ControlBeginOffset = offset };

        if ((edgeIndex != 0) || _closed)
        {
            var prevIndex = (edgeIndex - 1 + _edges.Count) % _edges.Count;

            _edges[prevIndex] = _edges[prevIndex] with
                { ControlEndOffset = -offset};
        }
    }

    public void SetControlEnd(int edgeIndex, Unit2D position)
    {
        var offset = position - Vertices.At(edgeIndex + 1).Position;

        _edges[edgeIndex] = _edges[edgeIndex] with
            { ControlEndOffset = offset };
        
        if ((edgeIndex != _edges.Count - 1) || _closed)
        {
            var nextIndex = (edgeIndex + 1) % _edges.Count;

            _edges[nextIndex] = _edges[nextIndex] with
                { ControlBeginOffset = -offset };
        }
    }

    public void Clear()
    {
        for (int i = _vertices.Count - 1; i >= 0; --i)
        {
            var key = _vertices.KeyAt(i);
            
            VertexRemoved?.Invoke(i, key);
        }

        for (int i = _edges.Count - 1; i >= 0; --i)
        {
            var key = _edges.KeyAt(i);
            
            EdgeRemoved?.Invoke(i, key);
        }
        
        _vertices.Clear();
        _edges.Clear();
        _closed = false;

        GeometryChanged?.Invoke();
    }

    public void Translate(Unit2D delta)
    {
        for (int i = 0; i < _vertices.Count; ++i)
        {
            var vertex = _vertices[i];
            
            _vertices.Set(i, vertex with { Position = vertex.Position + delta });
        }

        InvalidateAllPositions?.Invoke();
        GeometryChanged?.Invoke();
    }

    public void MirrorX(Unit centerY)
    {
        for (int i = 0; i < _vertices.Count; ++i)
        {
            var vertex = _vertices[i];
            var mirrored = vertex.Position with { Y = (centerY * 2) - vertex.Position.Y };

            _vertices.Set(i, vertex with { Position = mirrored });
        }
        
        for (int i = 0; i < _edges.Count; ++i)
        {
            var edge = _edges[i];

            _edges.Set(i, edge with
            {
                ControlBeginOffset = edge.ControlBeginOffset with { Y = -edge.ControlBeginOffset.Y },
                ControlEndOffset = edge.ControlEndOffset with { Y = -edge.ControlEndOffset.Y }
            });
        }

        InvalidateAllPositions?.Invoke();
        GeometryChanged?.Invoke();
    }

    public void MirrorY(Unit centerX)
    {
        for (int i = 0; i < _vertices.Count; ++i)
        {
            var vertex = _vertices[i];
            var mirrored = vertex.Position with { X = (centerX * 2) - vertex.Position.X };
            
            _vertices.Set(i, vertex with { Position = mirrored });
        }
        
        for (int i = 0; i < _edges.Count; ++i)
        {
            var edge = _edges[i];
            
            _edges.Set(i, edge with
            {
                ControlBeginOffset = edge.ControlBeginOffset with { X = -edge.ControlBeginOffset.X },
                ControlEndOffset = edge.ControlEndOffset with { X = -edge.ControlEndOffset.X }
            });
        }
        
        InvalidateAllPositions?.Invoke();
        GeometryChanged?.Invoke();
    }

    protected void AssignFromPolygon(Polygon other)
    {
        _vertices.AssignFrom(other._vertices);
        _edges.AssignFrom(other._edges);
        _closed = other._closed;

        GeometryChanged?.Invoke();
    }
    
    public Polygon DeepClone()
    {
        var clone = new Polygon();

        clone.AssignFromPolygon(this);

        return clone;
    }

    private void VertexReassigned(int index, ulong key, Vertex oldVertex, Vertex newVertex)
    {
        GeometryChanged?.Invoke();
    }

    private void EdgeReassigned(int index, ulong key, Edge oldEdge, Edge newEdge)
    {
        GeometryChanged?.Invoke();
    }
}


