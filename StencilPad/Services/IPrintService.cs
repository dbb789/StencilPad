using StencilPad.Models;

namespace StencilPad.Services;

public interface IPrintService
{
    Task<bool> PrintAsync(string documentName, Sheet sheet);
}
