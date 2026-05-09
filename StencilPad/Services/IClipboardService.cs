using StencilPad.Models;

namespace StencilPad.Services;

public interface IClipboardService
{
    void Copy(IEnumerable<ISheetElement> elements);
    IReadOnlyList<ISheetElement> Paste();
}
