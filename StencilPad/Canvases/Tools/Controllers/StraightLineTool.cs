using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;

namespace StencilPad.Canvases.Tools.Controllers;

public class StraightLineTool : LineTool
{
    public class Factory(Sheet Sheet,
                         OverlayContainer OverlayContainer,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService,
                         Factory<PolygonToolOverlay<Shape>> OverlayFactory) : IToolFactory
    {
        public string IconResource => "StraightLineTool";
        public string Tooltip => "Straight Lines";

        public ITool Create(IToolButton button)
        {
            return new StraightLineTool(Sheet,
                                        OverlayContainer,
                                        UnitSnapOverlay,
                                        OperationService,
                                        OverlayFactory);
        }
    }

    protected override bool IsCurved => false;

    private StraightLineTool(Sheet sheet,
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
