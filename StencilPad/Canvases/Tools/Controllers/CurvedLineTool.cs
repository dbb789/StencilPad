using System.Windows.Media;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;

namespace StencilPad.Canvases.Tools.Controllers;

public class CurvedLineTool : PolygonTool<LineToolOverlay<Shape>, Shape>
{
    public class Factory(Sheet Sheet,
                         OverlayContainer OverlayContainer,
                         ISettings Settings,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService,
                         Factory<LineToolOverlay<Shape>> OverlayFactory) : IToolFactory
    {
        public string IconResource => "CurvedLineTool";
        public string Tooltip => "Curved Lines";

        public ITool Create(IToolButton button)
        {
            return new CurvedLineTool(Sheet,
                                      OverlayContainer,
                                      Settings,
                                      UnitSnapOverlay,
                                      OperationService,
                                      OverlayFactory);
        }
    }

    private CurvedLineTool(Sheet sheet,
                           OverlayContainer overlayContainer,
                           ISettings settings,
                           IUnitSnapOverlay unitSnapOverlay,
                           IOperationService operationService,
                           Factory<LineToolOverlay<Shape>> overlayFactory)
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

        Overlay.IsCurved = true;
        Overlay.Element.LineColor = Color.FromArgb(127, 0, 0, 0);
    }
}
