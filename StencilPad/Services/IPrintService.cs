using System.Windows.Media;

namespace StencilPad.Services;

public interface IPrintService
{
    Task<bool> PrintAsync(string documentName, Action<DrawingContext> drawFunc);
}
