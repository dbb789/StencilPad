using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class PolygonSchema
{
    public VertexSchema[] V { get; set; } = [];
    public EdgeSchema[] E { get; set; } = [];
    public bool? C { get; set; }

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
            C = polygon.Closed ? true : null,
            V = vertices,
            E = edges
        };
    }

    public static Polygon Unpack(PolygonSchema data)
    {
        var polygon = new Polygon();

        foreach (var vertex in data.V)
        {
            polygon.AddVertex(VertexSchema.Unpack(vertex));
        }

        // NOTE: Closing a polygon will add an additional edge so perform this
        // first before edge property assignment below.
        if (data.C ?? false)
        {
            polygon.Close();
        }

        for (int i = 0; i < data.E.Length; i++)
        {
            polygon.Edges[i] = EdgeSchema.Unpack(data.E[i]);
        }


        return polygon;
    }
}
