using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public interface ITextWalker
{
    void SetTransform(UnitTransform transform);
    void SetStyle(TextStyle style);
    void SetBounds(UnitBounds? bounds);
    void SetText(string text);
}
