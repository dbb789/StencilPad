using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Services;

public static class LineStyleResourceLibrary
{
    public static readonly IReadOnlyList<(LineStyleResourceId, DashStyle)> ResourceList =
        new List<(LineStyleResourceId, DashStyle)>
        {
            ( LineStyleResourceId.Solid, DashStyles.Solid ),
            ( LineStyleResourceId.Dashes, DashStyles.Dash )
        };
}
