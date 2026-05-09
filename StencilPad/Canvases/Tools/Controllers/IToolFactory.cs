using  StencilPad.Canvases.Tools.Common;
using  StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Controllers;

public interface IToolFactory
{
    string IconResource { get; }
    string Tooltip { get; }
    
    ITool Create(IToolButton button, Sheet sheet, IToolContext context);
}
