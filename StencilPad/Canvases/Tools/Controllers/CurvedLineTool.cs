using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;

namespace StencilPad.Canvases.Tools.Controllers;

public class CurvedLineTool : LineTool
{
    public class Factory(Sheet Sheet,
                         OverlayContainer OverlayContainer,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService,
                         Factory<PolygonToolOverlay<Shape>> OverlayFactory) : IToolFactory
    {
        public string IconResource => "CurvedLineTool";
        public string Tooltip => "Curved Lines";

        public ITool Create(IToolButton button)
        {
            return new CurvedLineTool(Sheet,
                                      OverlayContainer,
                                      UnitSnapOverlay,
                                      OperationService,
                                      OverlayFactory);
        }
    }

    protected override bool IsCurved => true;

    private CurvedLineTool(Sheet sheet,
                             OverlayContainer overlayContainer,
                             IUnitSnapOverlay unitSnapOverlay,
                             IOperationService operationService,
                             Factory<PolygonToolOverlay<Shape>> overlayFactory)
        : base(sheet,
               overlayContainer,
               unitSnapOverlay,
               operationService,
               overlayFactory)
    {
        // ...
    }
}
