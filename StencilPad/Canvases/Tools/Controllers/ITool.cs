namespace StencilPad.Canvases.Tools.Controllers;

public interface ITool : IDisposable
{
    void ToolBegin();
    void ToolEnd();
}
