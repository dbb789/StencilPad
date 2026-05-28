using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class ElementGroupSchema : SheetElementSchema
{
    public SheetElementSchema[] Children { get; set; } = [];

    public static ElementGroupSchema Pack(ElementGroup elementGroup)
    {
        return new ElementGroupSchema
        {
            Children = elementGroup.Children
                .Select(Pack).Where(c => c is not null).ToArray()!,
                
            Transform = UnitTransformSchema.Pack(elementGroup.Transform)
        };
    }

    public override ISheetElement Unpack()
    {
        var children = Children.Select(c => c.Unpack()).ToArray();
        
        var group = new ElementGroup(children);
        
        group.Transform = UnitTransformSchema.Unpack(Transform);

        return group;
    }
}
