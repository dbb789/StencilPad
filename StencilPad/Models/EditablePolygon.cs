using StencilPad.Spatial;

namespace StencilPad.Models;

public class EditablePolygon : IPolygon, IHandleSet
{
    public AssignableList<Vertex> Vertices => _polygon.Vertices;
    public AssignableList<Edge> Edges => _polygon.Edges;
    public bool Closed => _polygon.Closed;

    private Polygon _polygon;
    private PolygonSelection _selection;

    public event Action? PolygonChanged;
    public event Action? SelectionChanged;
    public event Action? HandlesChanged;

    public EditablePolygon()
    {
        _polygon = new Polygon();
        _selection = new();

        AttachEvents();
    }

    public EditablePolygon(Polygon polygon)
    {
        _polygon = polygon.DeepClone();
        _selection = new();

        AttachEvents();
    }

    private void AttachEvents()
    {
        _polygon.VertexAdded += _selection.VertexAdded;
        _polygon.VertexRemoved += _selection.VertexRemoved;
        _polygon.Changed += () =>
        {
            PolygonChanged?.Invoke();
            HandlesChanged?.Invoke();
        };

        _selection.Changed += () => SelectionChanged?.Invoke();
    }

    public void AddVertex(Vertex vertex)
    {
        _polygon.AddVertex(vertex);
    }

    public void InsertVertex(int index, Vertex vertex)
    {
        _polygon.InsertVertex(index, vertex);
    }

    public void DeleteVertex(int index)
    {
        _polygon.DeleteVertex(index);
    }

    public void Open(int index)
    {
        _polygon.Open(index);
        _selection.Cycle((_polygon.Vertices.Count - 1) - index,
                         _polygon.Vertices.Count);
    }

    public void Close()
    {
        _polygon.Close();
    }

    public void Clear()
    {
        _polygon.Clear();
    }

    public void Translate(Unit2D delta)
    {
        _polygon.Translate(delta);
    }

    public void MirrorX(Unit centerY)
    {
        _polygon.MirrorX(centerY);
    }

    public void MirrorY(Unit centerX)
    {
        _polygon.MirrorY(centerX);
    }

    public IEnumerable<int> GetSelectedVertices()
    {
        return _selection.Selection.Where(x => x.Type == HandleType.Vertex)
                                   .Select(x => x.Index);
    }

    public IEnumerable<int> GetSelectedEdges()
    {
        var edges = new List<int>(_polygon.Edges.Count);

        for (int i = 0; i < _polygon.Edges.Count; i++)
        {
            if (_selection.Selection.Contains(Handle.Vertex(i)) &&
                _selection.Selection.Contains(Handle.Vertex((i + 1) % _polygon.Vertices.Count)))
            {
                edges.Add(i);
            }
        }

        return edges;
    }

    public PolygonSelection GetSelection()
    {
        return _selection.DeepClone();
    }

    public void SetSelection(PolygonSelection selection)
    {
        _selection.AssignFrom(selection);
    }

    public void ClearSelection()
    {
        _selection.Clear();
    }

    public IEnumerable<Handle> Handles
    {
        get
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                yield return Handle.Vertex(i);
            }

            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].Type == EdgeType.Bezier)
                {
                    yield return Handle.ControlBegin(i);
                    yield return Handle.ControlEnd(i);
                }
            }
        }
    }

    public Unit2D GetPoint(Handle handle)
    {
        switch (handle.Type)
        {
        case HandleType.Vertex:
            return Vertices[handle.Index].Position;
            
        case HandleType.ControlBegin:
            return Vertices[handle.Index].Position + Edges[handle.Index].ControlBeginOffset;
            
        case HandleType.ControlEnd:
            return Vertices.At(handle.Index + 1).Position + Edges[handle.Index].ControlEndOffset;
        }

        throw new ArgumentOutOfRangeException(nameof(handle));
    }

    public void SetPoint(Handle handle, Unit2D position)
    {
        switch (handle.Type)
        {
        case HandleType.Vertex:
            Vertices[handle.Index] = Vertices[handle.Index] with { Position = position };
            break;
            
        case HandleType.ControlBegin:
            Edges[handle.Index] = Edges[handle.Index] with
                { ControlBeginOffset = position - Vertices[handle.Index].Position };
            break;
            
        case HandleType.ControlEnd:
            Edges[handle.Index] = Edges[handle.Index] with
                { ControlEndOffset = position - Vertices.At(handle.Index + 1).Position };
            break;

        default:
            throw new ArgumentOutOfRangeException(nameof(handle));
        }
    }

    public IEnumerable<Handle> GetSelectedHandles()
    {
        return _selection.Selection;
    }

    public void SetSelectedHandles(IEnumerable<Handle> handles)
    {
        _selection.AssignFrom(handles);
    }

    public void AssignFrom(EditablePolygon other)
    {
        _polygon.AssignFromWithoutNotify(other._polygon);
        _selection.AssignFromWithoutNotify(other._selection);

        PolygonChanged?.Invoke();
        SelectionChanged?.Invoke();
        HandlesChanged?.Invoke();
    }

    public EditablePolygon DeepClone()
    {
        var editablePolygon = new EditablePolygon();

        editablePolygon.AssignFrom(this);
        
        return editablePolygon;
    }
}
