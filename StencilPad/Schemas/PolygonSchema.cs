using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class PolygonSchema
{
    public bool Closed { get; set; }
    public VertexSchema[] Vertices { get; set; } = [];
    public EdgeSchema[] Edges { get; set; } = [];

    public static PolygonSchema Pack(Polygon polygon)
    {
        return Pack((IPolygon)polygon);
    }
    
    public static PolygonSchema Pack(IPolygon polygon)
    {
        var vertices = new VertexSchema[polygon.Vertices.Count];
        
        for (int i = 0; i < polygon.Vertices.Count; i++)
        {
            vertices[i] = VertexSchema.Pack(polygon.Vertices[i]);
        }

        var edges = new EdgeSchema[polygon.Edges.Count];
        
        for (int i = 0; i < polygon.Edges.Count; i++)
        {
            edges[i] = EdgeSchema.Pack(polygon.Edges[i]);
        }

        return new PolygonSchema
        {
            Closed = polygon.Closed,
            Vertices = vertices,
            Edges = edges
        };
    }

    public static Polygon Unpack(PolygonSchema data)
    {
        var polygon = new Polygon();

        foreach (var vertex in data.Vertices)
        {
            polygon.AddVertex(VertexSchema.Unpack(vertex));
        }

        // NOTE: Closing a polygon will add an additional edge so perform this
        // first before edge property assignment below.
        if (data.Closed)
        {
            polygon.Close();
        }

        for (int i = 0; i < data.Edges.Length; i++)
        {
            polygon.Edges[i] = EdgeSchema.Unpack(data.Edges[i]);
        }


        return polygon;
    }
}
