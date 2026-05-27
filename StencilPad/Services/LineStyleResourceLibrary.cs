using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Services;

public static class LineStyleResourceLibrary
{
    public static readonly IReadOnlyDictionary<LineStyleResourceId, DashStyle> ResourceMap =
        new Dictionary<LineStyleResourceId, DashStyle>
        {
            { LineStyleResourceId.Solid, DashStyles.Solid },
            { LineStyleResourceId.Dashes, DashStyles.Dash }
        };
}
