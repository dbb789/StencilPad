using System.Windows.Media;

namespace StencilPad.Services;

public interface IResourceService
{
    public Geometry Get(GeometryResourceId id);
}
