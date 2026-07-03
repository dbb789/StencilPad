using System.Windows.Media;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;

namespace StencilPad.Canvases.Tools.Controllers;

public class RectTool : PolygonTool<RectToolOverlay<Shape>, Shape>
{
    public class Factory(Sheet Sheet,
                         OverlayContainer OverlayContainer,
                         ISettings Settings,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService,
                         Factory<RectToolOverlay<Shape>> OverlayFactory) : IToolFactory
    {
        public string IconResource => "RectTool";
        public string Tooltip => "Rectangles";

        public ITool Create(IToolButton button)
        {
            return new RectTool(Sheet,
                                OverlayContainer,
                                Settings,
                                UnitSnapOverlay,
                                OperationService,
                                OverlayFactory);
        }
    }

    private RectTool(Sheet sheet,
                     OverlayContainer overlayContainer,
                     ISettings settings,
                     IUnitSnapOverlay unitSnapOverlay,
                     IOperationService operationService,
                     Factory<RectToolOverlay<Shape>> overlayFactory)
        : base(sheet,
               overlayContainer,
               settings,
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
