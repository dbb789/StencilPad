using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using StencilPad.Canvases.Common;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class UnitSnapOverlay : ContentControl, IUnitSnapOverlay
{
    private static readonly Pen IndicatorPen;
    private static readonly IUnitSnapContext DefaultContext;

    public Unit2D? LastSnapPoint => _lastSnapPoint;

    private bool _isActive;
    private IViewport _viewport;
    private IUnitSnap _unitSnap;
    private IUnitSnapContext? _context;
    private Unit2D? _lastSnapPoint;
    
    static UnitSnapOverlay()
    {
        IndicatorPen = new Pen(new SolidColorBrush(Color.FromArgb(64, 0, 0, 0)), 1.0);
        IndicatorPen.Freeze();

        DefaultContext = EmptyUnitSnapContext.Instance;
    }

    public UnitSnapOverlay(IViewport viewport, IUnitSnap unitSnap)
    {
        _viewport = viewport;
        _unitSnap = unitSnap;
        _context = null;
    }

    public void Begin(IUnitSnapContext? context = null)
    {
        _context = context;
        _lastSnapPoint = null;
        _isActive = true;
    }

    public void End()
    {
        _context = null;
        _lastSnapPoint = null;
        _isActive = false;

        // Redraw without indicator.
        InvalidateVisual();        
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_isActive)
        {
            return;
        }

        var mousePos = _viewport.FromPoint(e.GetPosition(this));
        var snapped = _unitSnap.UnitSnap(mousePos, _context ?? DefaultContext);

        if (_lastSnapPoint != snapped)
        {
            _lastSnapPoint = snapped;
            InvalidateVisual();
        }
    }
    
    protected override void OnRender(DrawingContext dc)
    {
        if (!_isActive)
        {
            return;
        }

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_lastSnapPoint is null)
        {
            return;
        }

        var lastSnapPixels = _viewport.ToPoint(_lastSnapPoint.Value);

        dc.DrawLine(IndicatorPen,
                    lastSnapPixels + new Vector(-5, -5), lastSnapPixels + new Vector(5, 5));
        dc.DrawLine(IndicatorPen,
                    lastSnapPixels + new Vector(-5, 5), lastSnapPixels + new Vector(5, -5));
    }
}
