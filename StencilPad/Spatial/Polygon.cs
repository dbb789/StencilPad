namespace StencilPad.Spatial;

public class Polygon : IPolygon
{
    public AssignableList<Vertex> Vertices => _vertices;
    public AssignableList<Edge> Edges => _edges;
    public bool Closed => _closed;

    private readonly MutableAssignableList<Vertex> _vertices;
    private readonly MutableAssignableList<Edge> _edges;
    private bool _closed;

    // NOTE: Strictly defined to be invoked before Changed so that (eg) selected
    // indices can be corrected before the rest of the UI is forcibly redrawn.
    public event Action<int>? VertexAdded;
    public event Action<int>? VertexRemoved;

    public event Action? Changed;

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

        _vertices.Items.Insert(index, vertex);

        if (_vertices.Count > 1)
        {
            // Appends a new edge at the end if inserting at the end, otherwise
            // inserts the edge with the same index as the vertex.
            _edges.Items.Insert(Math.Min(index, _edges.Count), new Edge());
        }
        
        VertexAdded?.Invoke(index);
        InvokeChanged();
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

        _vertices.Items.RemoveAt(index);
        _edges.Items.RemoveAt(index);

        VertexRemoved?.Invoke(index);
        InvokeChanged();
    }
    
    public void Open(int index)
    {
        if (!_closed)
        {
            return;
        }

        var delta = (_vertices.Count - 1) - index;
        
        Cycle(_vertices.Items, delta);
        Cycle(_edges.Items, delta);
        
        _edges.Items.RemoveAt(_edges.Count - 1);
        _closed = false;

        InvokeChanged();
    }

    public void Close()
    {
        if (_closed || _vertices.Count <= 2)
        {
            return;
        }
        
        _edges.Items.Add(new Edge());
        _closed = true;

        InvokeChanged();
    }

    public void Clear()
    {
        _vertices.Items.Clear();
        _edges.Items.Clear();
        _closed = false;

        InvokeChanged();
    }

    public void Translate(Unit2D delta)
    {
        for (int i = 0; i < _vertices.Count; ++i)
        {
            var vertex = _vertices[i];
            
            _vertices.Items[i] = vertex with { Position = vertex.Position + delta };
        }

        InvokeChanged();
    }

    public void MirrorX(Unit centerY)
    {
        for (int i = 0; i < _vertices.Count; ++i)
        {
            var vertex = _vertices[i];
            var mirrored = vertex.Position with { Y = (centerY * 2) - vertex.Position.Y };

            _vertices.Items[i] = vertex with { Position = mirrored };
        }

        InvokeChanged();
    }

    public void MirrorY(Unit centerX)
    {
        for (int i = 0; i < _vertices.Count; ++i)
        {
            var vertex = _vertices[i];
            var mirrored = vertex.Position with { X = (centerX * 2) - vertex.Position.X };

            _vertices.Items[i] = vertex with { Position = mirrored };
        }

        InvokeChanged();
    }

    public void AssignFromWithoutNotify(Polygon other)
    {
        _vertices.Items.Clear();
        _vertices.Items.AddRange(other._vertices.Items);
        _edges.Items.Clear();
        _edges.Items.AddRange(other._edges.Items);
        _closed = other._closed;
    }
    
    public Polygon DeepClone()
    {
        var clone = new Polygon();

        clone.AssignFromWithoutNotify(this);

        return clone;
    }

    private void VertexReassigned(int index, Vertex oldVertex, Vertex newVertex)
    {
        InvokeChanged();
    }

    private void EdgeReassigned(int index, Edge oldEdge, Edge newEdge)
    {
        if (index != 0 || _closed)
        {
            var prevIndex = (index - 1 + _edges.Count) % _edges.Count;
            var prevEdge = _edges.Items[prevIndex];

            _edges.Items[prevIndex] = prevEdge with { ControlEndOffset = -_edges.At(index).ControlBeginOffset };
        }
        
        if ((index != _edges.Count - 1) || _closed)
        {
            var nextIndex = (index + 1) % _edges.Count;
            var nextEdge = _edges.Items[nextIndex];

            _edges.Items[nextIndex] = nextEdge with { ControlBeginOffset = -_edges.At(index).ControlEndOffset };
        }
        
        InvokeChanged();
    }

    private static void Cycle<T>(List<T> list, int delta)
    {
        if (list.Count == 0)
        {
            return;
        }
        
        var count = list.Count;
        var newItems = new List<T>(count);

        for (int i = 0; i < count; ++i)
        {
            newItems.Add(list[(i - delta + count) % count]);
        }

        list.Clear();
        list.AddRange(newItems);
    }
    
    private void InvokeChanged()
    {
        Changed?.Invoke();
    }
}
