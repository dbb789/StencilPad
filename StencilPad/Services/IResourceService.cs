using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Services;

public interface IResourceService
{
    IEnumerable<GeometryResourceId> GetGeometryResourceIds(GeometryResourceType type);
    IEnumerable<LineStyleResourceId> GetLineStyleResourceIds();

    GeometryResource Get(GeometryResourceId id);
    DashStyle Get(LineStyleResourceId id);
}
