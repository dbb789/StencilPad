using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Controllers.Actions;

public interface ISheetElementAction
{
    string Name { get; }
    
    bool IsVisible(Sheet sheet, IEnumerable<ISheetElement> elements);
    bool IsEnabled(Sheet sheet, IEnumerable<ISheetElement> elements);
    void Invoke(Sheet sheet, IEnumerable<ISheetElement> elements);
}
