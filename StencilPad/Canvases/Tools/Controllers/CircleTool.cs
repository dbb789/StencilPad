using System.Windows.Media;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;

namespace StencilPad.Canvases.Tools.Controllers;

public class CircleTool : PolygonTool<CircleToolOverlay<Shape>, Shape>
{
    public class Factory(Sheet Sheet,
                         OverlayContainer OverlayContainer,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService,
                         Factory<CircleToolOverlay<Shape>> OverlayFactory) : IToolFactory
    {
        public string IconResource => "CircleTool";
        public string Tooltip => "Rectangles";

        public ITool Create(IToolButton button)
        {
            return new CircleTool(Sheet,
                                OverlayContainer,
                                UnitSnapOverlay,
                                OperationService,
                                OverlayFactory);
        }
    }

    private CircleTool(Sheet sheet,
                       OverlayContainer overlayContainer,
                       IUnitSnapOverlay unitSnapOverlay,
                       IOperationService operationService,
                       Factory<CircleToolOverlay<Shape>> overlayFactory)
        : base(sheet,
               overlayContainer,
               unitSnapOverlay,
               operationService,
               overlayFactory)
    {
        // ...
    }

    public override void ToolBegin()
    {
        base.ToolBegin();
        
        if (Overlay is null)
        {
            return;
        }

        Overlay.Element.LineColor = Color.FromArgb(127, 0, 0, 0);
    }
}
