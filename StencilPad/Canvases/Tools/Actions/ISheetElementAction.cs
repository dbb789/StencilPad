using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Actions;

public interface ISheetElementAction
{
    bool IsVisible(Sheet sheet, IEnumerable<ISheetElement> elements);
    bool IsEnabled(Sheet sheet, IEnumerable<ISheetElement> elements);
    void Invoke(Sheet sheet, IEnumerable<ISheetElement> elements);
}
