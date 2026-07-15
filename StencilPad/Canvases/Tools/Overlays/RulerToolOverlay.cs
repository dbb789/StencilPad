using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Common;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Rendering;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class RulerToolOverlay : Canvas, IDisposable
{
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly IUnitSnapContext _unitSnapContext;
    private readonly Ruler _previewRuler;
    private readonly RulerResolver _previewResolver;
    private readonly ModelRenderer _previewRenderer;
    private readonly LockAxisState _lockAxisState;
    
    private Unit2D? _start;
    private Unit2D _currentSnappedMousePosition;

    public event Action<Unit2D, Unit2D>? OnRulerPlaced;

    public RulerToolOverlay(IViewport viewport,
                            IUnitSnap unitSnap,
                            ISettings settings,
                            IResourceService resourceService)
    {
        _viewport = viewport;
        _unitSnap = unitSnap;
        _unitSnapContext = new DefaultUnitSnapContext(viewport);
        _previewRuler = new Ruler { Color = Color.FromArgb(128, 0, 0, 0) };
        _previewResolver = new RulerResolver(_previewRuler, settings, resourceService);
        _previewRenderer = new ModelRenderer(resourceService);

        _previewResolver.Attach(_previewRenderer);
        
        _lockAxisState = new();

        _viewport.ViewportChanged += OnViewportChanged;
    }

    public void Dispose()
    {
        _viewport.ViewportChanged -= OnViewportChanged;

        _previewResolver.Detach();
        _previewResolver.Dispose();
        _previewRenderer.Dispose();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (_start is null)
        {
            _start = _currentSnappedMousePosition;
        }
        else if ((_start.Value - _currentSnappedMousePosition).Magnitude > Unit.FromMillimeters(1))
        {
            OnRulerPlaced?.Invoke(_previewRuler.Min, _previewRuler.Max);
            _start = null;
        }

        InvalidateVisual();

        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var mousePosition = e.GetPosition(this);

        _currentSnappedMousePosition = CurrentSnappedMouseOverPosition(mousePosition);

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

        var min = _start.Value;
        var max = _currentSnappedMousePosition;

        // Force the ruler to a sane consistent default orientation so that the
        // label isn't upside down.
        if (min.X.ApproximatelyEquals(max.X) && min.Y < max.Y)
        {
            (min, max) = (max, min);
        }
        else if (min.X > max.X)
        {
            (min, max) = (max, min);
        }

        _previewRuler.Min = min;
        _previewRuler.Max = max;

        dc.PushTransform(_viewport.MillimetersToPixelsTransform);
        _previewRenderer.Render(dc);
        dc.Pop();
    }

    private Unit2D CurrentSnappedMouseOverPosition(Point mousePosition)
    {
        var unitPosition = _viewport.FromPoint(mousePosition);
        var snapPosition = _unitSnap.UnitSnap(unitPosition, _unitSnapContext);
        
        if (snapPosition.HasValue)
        {
            unitPosition = snapPosition.Value;
        }
        
        if (_start is not null)
        {
            unitPosition = _lockAxisState.OnDragMove(ModifierUtil.IsLockToAxis(),
                                                     _viewport.FromPixels(12),
                                                     _start.Value,
                                                     unitPosition);
        }

        return unitPosition;
    }

    private void OnViewportChanged()
    {
        InvalidateVisual();
    }
}
