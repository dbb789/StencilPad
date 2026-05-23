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

    public void SetBounds(UnitBounds oldBounds, UnitBounds newBounds, UnitTransform transform)
    {
        if (_vertices.Count == 0)
        {
            return;
        }

        var oldSize = oldBounds.Size;
        var newSize = newBounds.Size;
        bool hasScaleX = oldSize.X.Millimeters > 1e-10;
        bool hasScaleY = oldSize.Y.Millimeters > 1e-10;

        double sx = hasScaleX ? newSize.X.Millimeters / oldSize.X.Millimeters : 1.0;
        double sy = hasScaleY ? newSize.Y.Millimeters / oldSize.Y.Millimeters : 1.0;
        double cornerScale = Math.Sqrt(Math.Abs(sx * sy));

        // Pre-compute new vertex positions before updating edges (offsets are relative to old positions)
        var newVertexPositions = new Unit2D[_vertices.Count];
        for (int i = 0; i < _vertices.Count; i++)
        {
            newVertexPositions[i] = RemapLocalPoint(_vertices[i].Position, oldBounds, newSize, hasScaleX, hasScaleY, transform);
        }

        // Remap bezier control offsets using old vertex positions and new vertex positions
        for (int i = 0; i < _edges.Count; i++)
        {
            var edge = _edges[i];
            if (edge.Type != EdgeType.Bezier)
            {
                continue;
            }

            var oldP1 = _vertices[i].Position + edge.ControlBeginOffset;
            var oldP2 = _vertices[(i + 1) % _vertices.Count].Position + edge.ControlEndOffset;

            var newP1 = RemapLocalPoint(oldP1, oldBounds, newSize, hasScaleX, hasScaleY, transform);
            var newP2 = RemapLocalPoint(oldP2, oldBounds, newSize, hasScaleX, hasScaleY, transform);

            _edges.Set(i, edge with
            {
                ControlBeginOffset = newP1 - newVertexPositions[i],
                ControlEndOffset = newP2 - newVertexPositions[(i + 1) % _vertices.Count]
            });
        }

        // Update vertices
        for (int i = 0; i < _vertices.Count; i++)
        {
            var v = _vertices[i];
            CornerSize newCornerSize = v.CornerSize.IsUnit
                ? CornerSize.FromUnit(Unit.FromMillimeters(v.CornerSize.Unit.Millimeters * cornerScale))
                : v.CornerSize;

            _vertices.Set(i, v with { Position = newVertexPositions[i], CornerSize = newCornerSize });
        }

        InvalidateAllPositions?.Invoke();
        InvokeGeometryChanged();
    }

    private static Unit2D RemapLocalPoint(Unit2D localPt,
                                          UnitBounds oldBounds,
                                          Unit2D newSize,
                                          bool hasScaleX,
                                          bool hasScaleY,
                                          UnitTransform transform)
    {
        var worldPt = transform.Apply(localPt);

        double relX = hasScaleX
            ? (worldPt.X.Millimeters - oldBounds.Min.X.Millimeters) / oldBounds.Size.X.Millimeters
            : 0.5;
        double relY = hasScaleY
            ? (worldPt.Y.Millimeters - oldBounds.Min.Y.Millimeters) / oldBounds.Size.Y.Millimeters
            : 0.5;

        var newWorldPt = new Unit2D(
            oldBounds.Min.X + Unit.FromMillimeters(relX * newSize.X.Millimeters),
            oldBounds.Min.Y + Unit.FromMillimeters(relY * newSize.Y.Millimeters));

        return transform.InverseApply(newWorldPt);
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

    public bool ContainsPoint(Unit2D point, Unit tolerance)
    {
        if (_vertices.Count < 2)
        {
            return false;
        }

        double toleranceMm = tolerance.Millimeters;
        
        if (_vertices.Count == 2)
        {
            return IsNearSegment(_vertices[0].Position, _vertices[1].Position, point, toleranceMm);
        }

        int winding = 0;

        for (int i = 0; i < _edges.Count; i++)
        {
            var a = _vertices[i].Position;
            var b = _vertices[(i + 1) % _vertices.Count].Position;
            var edge = _edges[i];

            if (edge.Type == EdgeType.Bezier)
            {
                winding += WindingBezier(a,
                                         a + edge.ControlBeginOffset,
                                         b + edge.ControlEndOffset,
                                         b,
                                         point,
                                         0);
            }
            else
            {
                winding += WindingSegment(a, b, point);
            }
        }

        if (!_closed)
        {
            winding += WindingSegment(_vertices[_vertices.Count - 1].Position, _vertices[0].Position, point);
        }

        return winding != 0;
    }

    private static bool IsNearSegment(Unit2D a, Unit2D b, Unit2D p, double toleranceMm)
    {
        double ax = a.X.Millimeters, ay = a.Y.Millimeters;
        double bx = b.X.Millimeters, by = b.Y.Millimeters;
        double px = p.X.Millimeters, py = p.Y.Millimeters;

        double dx = bx - ax, dy = by - ay;
        double lenSq = dx * dx + dy * dy;

        double t = lenSq > 1e-20 ? Math.Clamp(((px - ax) * dx + (py - ay) * dy) / lenSq, 0.0, 1.0) : 0.0;

        double cx = ax + t * dx, cy = ay + t * dy;
        double distSq = (px - cx) * (px - cx) + (py - cy) * (py - cy);

        return distSq <= toleranceMm * toleranceMm;
    }

    private static int WindingSegment(Unit2D a, Unit2D b, Unit2D p)
    {
        double ay = a.Y.Millimeters, by = b.Y.Millimeters, py = p.Y.Millimeters;

        double cross = (b.X.Millimeters - a.X.Millimeters) * (py - ay)
                     - (by - ay) * (p.X.Millimeters - a.X.Millimeters);

        if (ay <= py)
        {
            if (by > py && cross > 0) return 1;
        }
        else
        {
            if (by <= py && cross < 0) return -1;
        }

        return 0;
    }

    private static int WindingBezier(Unit2D p0, Unit2D p1, Unit2D p2, Unit2D p3, Unit2D point, int depth)
    {
        double py = point.Y.Millimeters;
        double minY = Math.Min(Math.Min(p0.Y.Millimeters, p1.Y.Millimeters),
                               Math.Min(p2.Y.Millimeters, p3.Y.Millimeters));
        double maxY = Math.Max(Math.Max(p0.Y.Millimeters, p1.Y.Millimeters),
                               Math.Max(p2.Y.Millimeters, p3.Y.Millimeters));

        if (py < minY || py > maxY)
        {
            return 0;
        }

        const double tolerance = 0.05;
        const int maxDepth = 16;

        if (depth >= maxDepth || BezierFlatness(p0, p1, p2, p3) < tolerance)
        {
            return WindingSegment(p0, p3, point);
        }

        var m01   = Midpoint(p0, p1);
        var m12   = Midpoint(p1, p2);
        var m23   = Midpoint(p2, p3);
        var m012  = Midpoint(m01, m12);
        var m123  = Midpoint(m12, m23);
        var m0123 = Midpoint(m012, m123);

        return WindingBezier(p0, m01, m012, m0123, point, depth + 1)
             + WindingBezier(m0123, m123, m23, p3, point, depth + 1);
    }

    private static double BezierFlatness(Unit2D p0, Unit2D p1, Unit2D p2, Unit2D p3)
    {
        double dx = p3.X.Millimeters - p0.X.Millimeters;
        double dy = p3.Y.Millimeters - p0.Y.Millimeters;
        double len = Math.Sqrt(dx * dx + dy * dy);

        if (len < 1e-10)
        {
            double d1x = p1.X.Millimeters - p0.X.Millimeters, d1y = p1.Y.Millimeters - p0.Y.Millimeters;
            double d2x = p2.X.Millimeters - p0.X.Millimeters, d2y = p2.Y.Millimeters - p0.Y.Millimeters;
            return Math.Max(Math.Sqrt(d1x * d1x + d1y * d1y), Math.Sqrt(d2x * d2x + d2y * d2y));
        }

        double invLen = 1.0 / len;
        double dist1 = Math.Abs((p1.X.Millimeters - p0.X.Millimeters) * dy
                               - (p1.Y.Millimeters - p0.Y.Millimeters) * dx) * invLen;
        double dist2 = Math.Abs((p2.X.Millimeters - p0.X.Millimeters) * dy
                               - (p2.Y.Millimeters - p0.Y.Millimeters) * dx) * invLen;

        return Math.Max(dist1, dist2);
    }

    private static Unit2D Midpoint(Unit2D a, Unit2D b)
    {
        return new Unit2D(Unit.FromMillimeters((a.X.Millimeters + b.X.Millimeters) * 0.5),
                          Unit.FromMillimeters((a.Y.Millimeters + b.Y.Millimeters) * 0.5));
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


