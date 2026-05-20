using StencilPad.Spatial;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Canvases.UI;
using StencilPad.Rendering;

namespace StencilPad.Canvases.Tools.Common;

public interface IToolContext
{
    ToolOverlay ToolOverlay { get; }
    CanvasGrid CanvasGrid { get; }
    SheetRenderer SheetRenderer { get; }
    IEditOverlayRenderer EditOverlayRenderer { get; }
    IViewport Viewport { get; }
    IHandleMap HandleMap { get; }
    IRubberBand RubberBand { get; }
    IUnitSnap UnitSnap { get; }
    IUnitSnapOverlay UnitSnapOverlay { get; }
    
    event Action? SelectAllRequested;
    event Action? ClearSelectionRequested;
}
