namespace StencilPad.Canvases.Tools.Overlays;

public interface IToolButton : IDisposable
{
    bool IsEnabled { get; set; }
}
