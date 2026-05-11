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
                .Select(Pack).Where(c => c is not null).ToArray()!
        };
    }

    public override ISheetElement Unpack()
    {
        var children = Children.Select(c => c.Unpack()).ToArray();
        
        return new ElementGroup(children);
    }
}
