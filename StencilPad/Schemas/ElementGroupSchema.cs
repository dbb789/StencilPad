using StencilPad.Models;

namespace StencilPad.Schemas;

public class ElementGroupSchema : SheetElementSchema
{
    public SheetElementSchema[] C { get; set; } = [];

    public static ElementGroupSchema Pack(ElementGroup elementGroup)
    {
        return new ElementGroupSchema
        {
            C = elementGroup.Children
                .Select(Pack).Where(c => c is not null).ToArray()!,
                
            Trns = UnitTransformSchema.Pack(elementGroup.Transform)
        };
    }

    public override ISheetElement Unpack()
    {
        var children = C.Select(c => c.Unpack()).ToArray();
        
        var group = new ElementGroup(children);
        
        group.Transform = UnitTransformSchema.Unpack(Trns);

        return group;
    }
}
