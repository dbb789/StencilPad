using StencilPad.Models;
using StencilPad.Models.Resolvers;

namespace StencilPad.Services;

public interface IResourceService : IResourceSet
{
    IEnumerable<GeometryResourceId> GetGeometryResourceIds(GeometryResourceType type);
    IEnumerable<LineStyleResourceId> GetLineStyleResourceIds();
}
