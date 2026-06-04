using System.Windows.Media;

namespace StencilPad.Models.Resolvers;

public interface IResourceSet
{
    GeometryResource Get(GeometryResourceId id);
    DashStyle Get(LineStyleResourceId id);
}
