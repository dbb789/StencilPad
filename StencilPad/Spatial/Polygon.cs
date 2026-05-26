namespace StencilPad.Spatial;

public class Polygon : IPolygon
{
    public IKeyedList<Vertex> Vertices => _vertices;
    public IKeyedList<Edge> Edges => _edges;
    public bool Closed => _closed;

    public IPolygonResolver Resolver => _resolver;
    
    private readonly KeyedList<Vertex> _vertices;
    private readonly KeyedList<Edge> _edges;
    private bool _closed;
    
    private readonly PolygonResolver _resolver;
    
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
        _resolver = new(this);
        
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
        
        InvokeGeometryChanged();
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

        InvokeGeometryChanged();
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
        InvokeGeometryChanged();
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
        InvokeGeometryChanged();
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

    public UnitBounds CalculateBounds() => CalculateBounds(UnitTransform.Identity);

    public UnitBounds CalculateBounds(UnitTransform transform)
    {
        if (_vertices.Count == 0)
        {
            return UnitBounds.Empty;
        }

        var first = transform.Apply(_vertices[0].Position);
        var bounds = UnitBounds.FromMinMax(first, first);

        for (int i = 1; i < _vertices.Count; i++)
        {
            bounds = bounds.Extend(transform.Apply(_vertices[i].Position));
        }

        for (int i = 0; i < _edges.Count; i++)
        {
            if (_edges[i].Type != EdgeType.Bezier)
            {
                continue;
            }

            var p0 = transform.Apply(_vertices[i].Position);
            var p3 = transform.Apply(_vertices[(i + 1) % _vertices.Count].Position);
            var p1 = transform.Apply(_vertices[i].Position + _edges[i].ControlBeginOffset);
            var p2 = transform.Apply(_vertices[(i + 1) % _vertices.Count].Position + _edges[i].ControlEndOffset);

            double p0x = p0.X.Millimeters, p1x = p1.X.Millimeters, p2x = p2.X.Millimeters, p3x = p3.X.Millimeters;
            double p0y = p0.Y.Millimeters, p1y = p1.Y.Millimeters, p2y = p2.Y.Millimeters, p3y = p3.Y.Millimeters;

            double minX = Math.Min(p0x, p3x), maxX = Math.Max(p0x, p3x);
            double minY = Math.Min(p0y, p3y), maxY = Math.Max(p0y, p3y);

            ExtendBezierAxis(p0x, p1x, p2x, p3x, ref minX, ref maxX);
            ExtendBezierAxis(p0y, p1y, p2y, p3y, ref minY, ref maxY);

            bounds = bounds.Extend(new Unit2D(Unit.FromMillimeters(minX), Unit.FromMillimeters(minY)));
            bounds = bounds.Extend(new Unit2D(Unit.FromMillimeters(maxX), Unit.FromMillimeters(maxY)));
        }

        return bounds;
    }

    private static void ExtendBezierAxis(double p0, double p1, double p2, double p3, ref double min, ref double max)
    {
        // Solve B'(t) = 0: 3[At² + Bt + C] = 0
        // A = -p0 + 3p1 - 3p2 + p3
        // B = 2(p0 - 2p1 + p2)
        // C = p1 - p0
        double a = -p0 + 3 * p1 - 3 * p2 + p3;
        double b = 2 * (p0 - 2 * p1 + p2);
        double c = p1 - p0;

        if (Math.Abs(a) < 1e-12)
        {
            if (Math.Abs(b) > 1e-12)
            {
                GetBezierMinMax(p0, p1, p2, p3, -c / b, ref min, ref max);
            }
            return;
        }

        double discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            return;
        }

        double sqrtD = Math.Sqrt(discriminant);
        
        GetBezierMinMax(p0, p1, p2, p3, (-b + sqrtD) / (2 * a), ref min, ref max);
        GetBezierMinMax(p0, p1, p2, p3, (-b - sqrtD) / (2 * a), ref min, ref max);
    }

    private static void GetBezierMinMax(double p0,
                                        double p1,
                                        double p2,
                                        double p3,
                                        double t,
                                        ref double min,
                                        ref double max)
    {
        if (t <= 0 || t >= 1)
        {
            return;
        }

        double t_2 = t * t;
        double t_3 = t_2 * t;
        double mt = 1 - t;
        double mt_2 = mt * mt;
        double mt_3 = mt_2 * mt;

        double val = mt_3 * p0 + 3 * mt_2 * t * p1 + 3 * mt * t_2 * p2 + t_3 * p3;

        min = Math.Min(min, val);
        max = Math.Max(max, val);
    }

    public void SetBounds(UnitBounds oldBounds,
                          UnitBounds newBounds,
                          UnitTransform transform)
    {
        if (_vertices.Count == 0)
        {
            return;
        }

        var oldVertices = _vertices.ToArray();
        
        for (int i = 0; i < _vertices.Count; ++i)
        {
            var vertex = _vertices[i];
            var newPosition = RemapPoint(vertex.Position, oldBounds, newBounds, transform);
            
            _vertices.Set(i, vertex with { Position = newPosition });
        }

        for (int i = 0; i < _edges.Count; ++i)
        {
            var edge = _edges[i];
            
            var controlBegin = oldVertices[i].Position + edge.ControlBeginOffset;
            var newControlBegin = RemapPoint(controlBegin, oldBounds, newBounds, transform);

            var controlEnd = oldVertices[(i + 1) % _vertices.Count].Position + edge.ControlEndOffset;
            var newControlEnd = RemapPoint(controlEnd, oldBounds, newBounds, transform);

            _edges.Set(i, edge with
            {
                ControlBeginOffset = newControlBegin - _vertices[i].Position,
                ControlEndOffset = newControlEnd - _vertices[(i + 1) % _vertices.Count].Position
            });
        }
        
        InvalidateAllPositions?.Invoke();
        InvokeGeometryChanged();
    }

    private Unit2D RemapPoint(Unit2D localPoint,
                              UnitBounds oldBounds,
                              UnitBounds newBounds,
                              UnitTransform transform)
    {
        var worldPoint = transform.Apply(localPoint);
        
        double tX = Unit.InverseLerp(oldBounds.Min.X, oldBounds.Max.X, worldPoint.X);
        double tY = Unit.InverseLerp(oldBounds.Min.Y, oldBounds.Max.Y, worldPoint.Y);
        
        return new Unit2D(Unit.Lerp(newBounds.Min.X, newBounds.Max.X, tX),
                          Unit.Lerp(newBounds.Min.Y, newBounds.Max.Y, tY));
    }

    public Unit2D CalculateMidpoint()
    {
        if (_vertices.Count == 0)
        {
            return Unit2D.Zero;
        }

        var sum = Unit2D.Zero;

        for (int i = 0; i < _vertices.Count; i++)
        {
            sum += _vertices[i].Position;
        }

        return sum / _vertices.Count;
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

        InvokeGeometryChanged();
    }

    public void Translate(Unit2D delta)
    {
        for (int i = 0; i < _vertices.Count; ++i)
        {
            var vertex = _vertices[i];
            
            _vertices.Set(i, vertex with { Position = vertex.Position + delta });
        }

        InvalidateAllPositions?.Invoke();
        InvokeGeometryChanged();
    }

    public void Transform(UnitTransform transform)
    {
        for (int i = 0; i < _vertices.Count; ++i)
        {
            var vertex = _vertices[i];
            _vertices.Set(i, vertex with { Position = transform.Apply(vertex.Position) });
        }

        for (int i = 0; i < _edges.Count; ++i)
        {
            var edge = _edges[i];
            _edges.Set(i, edge with
            {
                ControlBeginOffset = transform.Rotate(edge.ControlBeginOffset),
                ControlEndOffset = transform.Rotate(edge.ControlEndOffset)
            });
        }

        InvalidateAllPositions?.Invoke();
        InvokeGeometryChanged();
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
        InvokeGeometryChanged();
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
        InvokeGeometryChanged();
    }

    protected void AssignFromPolygon(Polygon other)
    {
        _vertices.AssignFrom(other._vertices);
        _edges.AssignFrom(other._edges);
        _closed = other._closed;

        InvokeGeometryChanged();
    }
    
    public Polygon DeepClone()
    {
        var clone = new Polygon();

        clone.AssignFromPolygon(this);

        return clone;
    }

    private void VertexReassigned(int index, ulong key, Vertex oldVertex, Vertex newVertex)
    {
        InvokeGeometryChanged();
    }

    private void EdgeReassigned(int index, ulong key, Edge oldEdge, Edge newEdge)
    {
        InvokeGeometryChanged();
    }

    private void InvokeGeometryChanged()
    {
        _resolver.MarkGeometryDirty();
        GeometryChanged?.Invoke();
    }
}


