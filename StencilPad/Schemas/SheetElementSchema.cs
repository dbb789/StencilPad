using System.Text.Json.Serialization;
using StencilPad.Models;

namespace StencilPad.Schemas;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "T")]
[JsonDerivedType(typeof(ElementGroupSchema), "Grp")]
[JsonDerivedType(typeof(ShapeSchema), "Shp")]
[JsonDerivedType(typeof(MarkerPathSchema), "MrkP")]
[JsonDerivedType(typeof(RulerSchema), "Rlr")]
[JsonDerivedType(typeof(TextElementSchema), "Txt")]
[JsonDerivedType(typeof(ImageElementSchema), "Img")]
public abstract class SheetElementSchema
{
    public UnitTransformSchema Trns { get; set; } = new();

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
