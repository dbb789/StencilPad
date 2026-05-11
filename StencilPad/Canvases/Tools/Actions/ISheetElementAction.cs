using StencilPad.Canvases.Tools.Common;
using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Actions;

public interface ISheetElementAction
{
    string Name { get; }
    
    bool IsVisible(IToolContext context, Sheet sheet, IEnumerable<ISheetElement> elements);
    bool IsEnabled(IToolContext context, Sheet sheet, IEnumerable<ISheetElement> elements);
    void Invoke(IToolContext context, Sheet sheet, IEnumerable<ISheetElement> elements);
}
