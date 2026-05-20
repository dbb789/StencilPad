using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Widgets;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class SelectionToolOverlay : FrameworkElement, IUnitSnapContext, IDisposable
{
    private IToolContext _context;
    private Sheet _sheet;
    private HashSet<IHandleSource> _selectedSources;

    private Point? _dragPixelStart;
    private Unit2D _dragUnitStart;
    private UnitBounds _dragSelectionBounds;
    private bool _draggingSelection;
    
    private bool _redrawPending;
    private Pen _elementPen;
    private Brush _elementFill;
    private Pen _groupPen;
    private Brush _groupFill;
    
    public event Action<Unit2D>? SelectionDragged;
    public event Action<Unit2D>? PointSelected;
    
    public event Action<ISheetElementAction>? ActionInvoked;

    public SelectionToolOverlay(IToolContext context,
                                Sheet sheet,
                                IEnumerable<ISheetElementAction?> actions)
    {
        _context = context;
        _sheet = sheet;
        _selectedSources = [];
        _sheet.Selection.CollectionChanged += SelectionChanged;

        foreach (var selected in _sheet.Selection)
        {
            _selectedSources.Add(selected.HandleSource);

            if (_context.SheetRenderer.TryGetElementRenderer(selected, out var renderer))
            {
                renderer.RendererDirty += ForceRedraw;
            }
        }

        _context.UnitSnapOverlay.Begin(this);

        _elementPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)), 0.4);
        _elementPen.Freeze();

        _elementFill = new SolidColorBrush(Color.FromArgb(10, 0, 0, 255));
        _elementFill.Freeze();
        
        _groupPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 128, 255)), 0.4);
        _groupPen.Freeze();

        _groupFill = new SolidColorBrush(Color.FromArgb(10, 0, 128, 255));
        _groupFill.Freeze();
        
        ContextMenu = new ContextMenu();
        ContextMenuOpening += (s, e) => RebuildContextMenu(s, e, actions);

        Loaded += (s, e) =>
        {
            CompositionTarget.Rendering += OnRendering;
        };

        Unloaded += (s, e) =>
        {
            CompositionTarget.Rendering -= OnRendering;
        };
    }

    public void Dispose()
    {
        _context.UnitSnapOverlay.End();

        _sheet.Selection.CollectionChanged -= SelectionChanged;

        foreach (var selected in _sheet.Selection)
        {
            _selectedSources.Remove(selected.HandleSource);
            
            if (_context.SheetRenderer.TryGetElementRenderer(selected, out var renderer))
            {
                renderer.RendererDirty -= ForceRedraw;
            }
        }
    }
    
    private void RebuildContextMenu(object sender,
                                    ContextMenuEventArgs e,
                                    IEnumerable<ISheetElementAction?> actions)
    {
        if (!ContextMenuUtil.RebuildContextMenu(ContextMenu,
                                                _context,
                                                _sheet,
                                                _sheet.Selection,
                                                actions,
                                                ActionInvoked))
        {
            e.Handled = true;
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        var mousePosition = e.GetPosition(this);

        if (PointIsOverSelection(_context.Viewport.FromPoint(mousePosition)))
        {
            var bounds = GetSelectionBounds();
            
            if (bounds.HasValue)
            {
                _dragPixelStart = mousePosition;
                _dragUnitStart = _context.Viewport.FromPoint(mousePosition);
                _dragSelectionBounds = bounds.Value;
                e.Handled = true;
            }
            return;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_dragPixelStart.HasValue)
        {
            var mousePosition = e.GetPosition(this);
            var pixelDelta = mousePosition - _dragPixelStart.Value;

            if (_draggingSelection ||
                Math.Abs(pixelDelta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(pixelDelta.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _draggingSelection = true;

                var currentUnit = _context.Viewport.FromPoint(mousePosition);
                var displacement = currentUnit - _dragUnitStart;

                var currentBounds = GetSelectionBounds();
                if (currentBounds.HasValue)
                {
                    var delta = SnapDelta(_dragSelectionBounds, displacement, currentBounds.Value);
                    SelectionDragged?.Invoke(delta);
                }

                e.Handled = true;
            }
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!_draggingSelection)
        {
            var mousePosition = _context.Viewport.FromPoint(e.GetPosition(this));
            
            PointSelected?.Invoke(mousePosition);
            e.Handled = true;
        }

        if (_dragPixelStart.HasValue)
        {
            _dragPixelStart = null;
            e.Handled = true;
        }
        
        _draggingSelection = false;
    }
    
    private Unit2D SnapDelta(UnitBounds dragBounds, Unit2D displacement, UnitBounds currentBounds)
    {
        // The 4 corners of the selection as they would be after applying the raw displacement.
        // Each desired corner has a corresponding current corner (same index).
        Span<Unit2D> desiredCorners =
        [
            dragBounds.Min + displacement,
            new Unit2D(dragBounds.Max.X, dragBounds.Min.Y) + displacement,
            new Unit2D(dragBounds.Min.X, dragBounds.Max.Y) + displacement,
            dragBounds.Max + displacement,
        ];

        Span<Unit2D> currentCorners =
        [
            currentBounds.Min,
            new Unit2D(currentBounds.Max.X, currentBounds.Min.Y),
            new Unit2D(currentBounds.Min.X, currentBounds.Max.Y),
            currentBounds.Max,
        ];

        int bestIndex = 0;
        double bestError = double.MaxValue;

        for (int i = 0; i < desiredCorners.Length; i++)
        {
            var snapped = desiredCorners[i];
            var snapPosition = _context.UnitSnap.UnitSnap(desiredCorners[i], this);

            if (snapPosition.HasValue)
            {
                snapped = snapPosition.Value;
            }
            
            var error = (snapped - desiredCorners[i]).Magnitude.Millimeters;
            
            if (error < bestError)
            {
                bestError = error;
                bestIndex = i;
            }
        }

        var desiredCorner = desiredCorners[bestIndex];
        var snappedBest = _context.UnitSnap.UnitSnap(desiredCorners[bestIndex], this);
        
        if (snappedBest.HasValue)
        {
            desiredCorner = snappedBest.Value;
        }

        return desiredCorner - currentCorners[bestIndex];
    }

    private bool PointIsOverSelection(Unit2D point)
    {
        foreach (var selected in _sheet.Selection)
        {
            if (_context.SheetRenderer.TryGetElementRenderer(selected, out var renderer) &&
                renderer.HitTest(point))
            {
                return true;
            }
        }

        return false;
    }
    
    private UnitBounds? GetSelectionBounds()
    {
        UnitBounds? selectionBounds = null;

        foreach (var selected in _sheet.Selection)
        {
            if (_context.SheetRenderer.TryGetElementRenderer(selected, out var renderer))
            {
                if (selectionBounds.HasValue)
                {
                    selectionBounds = UnitBounds.Union(selectionBounds.Value, renderer.SelectionBounds);
                }
                else
                {
                    selectionBounds = renderer.SelectionBounds;
                }
            }
        }

        return selectionBounds;
    }

    private void SelectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is ISheetElement element)
                {
                    _selectedSources.Remove(element.HandleSource);

                    if (_context.SheetRenderer.TryGetElementRenderer(element, out var renderer))
                    {
                        renderer.RendererDirty -= ForceRedraw;
                    }
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is ISheetElement element)
                {
                    _selectedSources.Add(element.HandleSource);

                    if (_context.SheetRenderer.TryGetElementRenderer(element, out var renderer))
                    {
                        renderer.RendererDirty += ForceRedraw;
                    }
                }
            }
        }
        
        ForceRedraw();
    }
    
    public bool CanUnitSnapTo(IHandleSource source)
    {
        return !_selectedSources.Contains(source);
    }
    
    public bool CanUnitSnapTo(Handle handle)
    {
        return true;
    }

    private void ForceRedraw()
    {
        _redrawPending = true;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (_redrawPending)
        {
            InvalidateVisual();
            _redrawPending = false;
        }
    }
    
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        dc.PushTransform(_context.Viewport.MillimetersToPixelsTransform);
        
        foreach (var selected in _sheet.Selection)
        {
            if (_context.SheetRenderer.TryGetElementRenderer(selected, out var renderer))
            {
                var bounds = renderer.SelectionBounds;

                if (selected is ElementGroup)
                {
                    dc.DrawRectangle(_groupFill, _groupPen, bounds.Millimeters);
                }
                else
                {
                    dc.DrawRectangle(_elementFill, _elementPen, bounds.Millimeters);
                }
            }
        }

        dc.Pop();
    }
}
