using StencilPad.Models;
using System.Windows.Media;

namespace StencilPad.Services;

public interface IResourceService
{
    GeometryResource Get(GeometryResourceId id);
    DashStyle Get(LineStyleResourceId id);
}
