using System.Text.Json.Serialization;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(ElementGroupSchema), "Group")]
[JsonDerivedType(typeof(ShapeSchema), "Shape")]
[JsonDerivedType(typeof(MarkerPathSchema), "MarkerPath")]
[JsonDerivedType(typeof(RulerSchema), "Ruler")]
[JsonDerivedType(typeof(TextElementSchema), "Text")]
[JsonDerivedType(typeof(ImageElementSchema), "Image")]
public abstract class SheetElementSchema
{
    public UnitTransformSchema Transform { get; set; } = new();

    public abstract ISheetElement Unpack();

    public static SheetElementSchema? Pack(ISheetElement element)
    {
        return element switch
        {
            ElementGroup g => ElementGroupSchema.Pack(g),
            Shape s => ShapeSchema.Pack(s),
            MarkerPath s => MarkerPathSchema.Pack(s),
            Ruler s => RulerSchema.Pack(s),
            TextElement t => TextElementSchema.Pack(t),
            ImageElement i => ImageElementSchema.Pack(i),
            _ => null
        };
    }
}
