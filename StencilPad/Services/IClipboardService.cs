using StencilPad.Models;

namespace StencilPad.Services;

public interface IClipboardService
{
    void Copy(Sheet sheet);
    void Cut(Sheet sheet);
    void Paste(Sheet sheet);
}
