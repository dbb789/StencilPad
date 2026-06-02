using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public interface IStyledGeometryWalker : IDisposable
{
    void AddResolver(int id,
                     IGeometryResolver resolver,
                     GeometryStyle style,
                     UnitTransform transform);

    void RemoveResolver(int id);

    void UpdateResolver(int id,
                        IGeometryResolver resolver);
    
    void UpdateResolver(int id,
                        GeometryStyle style,
                        UnitTransform transform);

    void AddResource(int id,
                     GeometryResourceId resource,
                     GeometryStyle style,
                     UnitTransform transform);

    void RemoveResource(int id);

    void UpdateResource(int id,
                        GeometryResourceId resource);

    void UpdateResource(int id,
                        GeometryStyle style,
                        UnitTransform transform);
}
