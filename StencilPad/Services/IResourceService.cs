using StencilPad.Models;
using System.Windows.Media;

namespace StencilPad.Services;

public interface IResourceService
{
    IEnumerable<GeometryResourceId> GetResourceIds(GeometryResourceType type);

    GeometryResource Get(GeometryResourceId id);
    DashStyle Get(LineStyleResourceId id);
}
