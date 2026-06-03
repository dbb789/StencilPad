using System.Windows.Media;

namespace StencilPad.Models.Resolvers;

public readonly record struct TextStyle
{
    public string Font { get; init; }
    public double Size { get; init; }
    public Color Color { get; init; }

    public TextStyle()
    {
        Font = "Arial";
        Size = 12;
        Color = Color.FromArgb(255, 0, 0, 0);
    }
}
