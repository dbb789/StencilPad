using StencilPad.Models;
using System.Windows.Media;

namespace StencilPad.Services;

public interface IResourceService
{
    Geometry Get(GeometryResourceId id);
    DashStyle Get(LineStyleResourceId id);
}
