using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public interface IStyledGeometryWalker : IDisposable
{
    void SetStyle(GeometryStyle style);
    void SetTransform(UnitTransform transform);

    void Create(int id,
                GeometrySet geometrySet);

    void Update(int id,
                GeometrySet geometrySet);
    
    void Destroy(int id);
}
