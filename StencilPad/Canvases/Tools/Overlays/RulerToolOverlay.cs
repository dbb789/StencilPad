using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Models;
using StencilPad.Rendering;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class RulerToolOverlay : Canvas, IDisposable
{
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly Ruler _previewRuler;
    private readonly RulerRenderer _previewRenderer;

    private Unit2D? _start;
    private Point _currentMousePosition;

    public event Action<Unit2D, Unit2D>? OnRulerPlaced;

    public RulerToolOverlay(IViewport viewport, IUnitSnap unitSnap, IResourceService resourceService)
    {
        _viewport = viewport;
        _unitSnap = unitSnap;
        _previewRuler = new Ruler();
        _previewRenderer = new RulerRenderer(_previewRuler, resourceService);

        _viewport.ViewportChanged += OnViewportChanged;
    }

    public void Dispose()
    {
        _viewport.ViewportChanged -= OnViewportChanged;
        _previewRenderer.Dispose();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        _currentMousePosition = e.GetPosition(this);

        var unitPosition = Snap(_viewport.FromPoint(_currentMousePosition));

        if (_start is null)
        {
            _start = unitPosition;
        }
        else
        {
            OnRulerPlaced?.Invoke(_start.Value, unitPosition);
            _start = null;
        }

        InvalidateVisual();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _currentMousePosition = e.GetPosition(this);

        if (_start is not null)
        {
            InvalidateVisual();
        }
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_start is null)
        {
            return;
        }

        _previewRuler.Min = _start.Value;
        _previewRuler.Max = Snap(_viewport.FromPoint(_currentMousePosition));

        dc.PushTransform(_viewport.MillimetersToPixelsTransform);
        _previewRenderer.Render(dc);
        dc.Pop();
    }

    private Unit2D Snap(Unit2D position)
    {
        return _unitSnap.UnitSnap(position, EmptyUnitSnapContext.Instance) ?? position;
    }

    private void OnViewportChanged()
    {
        InvalidateVisual();
    }
}
