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

    private DragState<ISheetElement> _dragState;

    private Pen _elementPen;
    private Brush _elementFill;
    private Pen _groupPen;
    private Brush _groupFill;
    
    public event Action<Unit2D>? SelectionDragged;    
    public event Action<ISheetElementAction>? ActionInvoked;

    public SelectionToolOverlay(IToolContext context,
                                Sheet sheet,
                                IEnumerable<ISheetElementAction?> actions)
    {
        _context = context;
        _sheet = sheet;
        _sheet.Selection.CollectionChanged += SelectionChanged;
        _dragState = new();
        
        foreach (var selected in _sheet.Selection)
        {
            if (_context.SheetRenderer.TryGetElementRenderer(selected, out var renderer))
            {
                renderer.RendererDirty += ForceRedraw;
            }
        }

        _elementPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)), 2);
        _elementPen.Freeze();

        _elementFill = new SolidColorBrush(Color.FromArgb(10, 0, 0, 255));
        _elementFill.Freeze();
        
        _groupPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 128, 255)), 2);
        _groupPen.Freeze();

        _groupFill = new SolidColorBrush(Color.FromArgb(10, 0, 128, 255));
        _groupFill.Freeze();
        
        ContextMenu = new ContextMenu();
        ContextMenuOpening += (s, e) => RebuildContextMenu(s, e, actions);
    }

    public void Dispose()
    {
        _sheet.Selection.CollectionChanged -= SelectionChanged;

        foreach (var selected in _sheet.Selection)
        {
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

        var elementUnderMouse = PointOverSelection(_context.Viewport.FromPoint(mousePosition));

        if (elementUnderMouse != null)
        {
            var elementBounds = GetElementBounds(elementUnderMouse);
            
            _dragState.OnDragStart(mousePosition,
                                   elementUnderMouse,
                                   elementBounds.Center);

            CaptureMouse();
            e.Handled = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var mousePosition = e.GetPosition(VisualTreeHelper.GetParent(this) as UIElement);

        if (!_dragState.DragStarted)
        {
            return;
        }

        var elementBounds = GetElementBounds(_dragState.DraggedElement);
        var dragResult = _dragState.OnDragMove(_context.Viewport,
                                               mousePosition);
        
        if (dragResult is null)
        {
            return;
        }
        
        var targetPosition = dragResult.Value.TargetElementPosition;
        var targetBounds = UnitBounds.FromCenterSize(targetPosition, elementBounds.Size);
        var snappedCenter = SnapBoundsCenter(targetBounds);
        var delta = snappedCenter - elementBounds.Center;
        
        SelectionDragged?.Invoke(delta);
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        _dragState.OnDragEnd();
        
        ReleaseMouseCapture();
        e.Handled = true;

        // Clear drag fill.
        ForceRedraw();
    }

    private Unit2D SnapBoundsCenter(UnitBounds bounds)
    {
        Span<Unit2D> corners =
        [
            bounds.NW, bounds.NE, bounds.SW, bounds.SE
        ];

        int closestIndex = -1;
        Unit2D smallestDelta = Unit2D.Square(Unit.FromMillimeters(1000));

        for (int i = 0; i < corners.Length; ++i)
        {
            var snapPosition = _context.UnitSnap.UnitSnap(corners[i], this);

            if (snapPosition.HasValue)
            {
                var delta = snapPosition.Value - corners[i];

                if (delta.SqrMagnitude < smallestDelta.SqrMagnitude)
                {
                    smallestDelta = delta;
                    closestIndex = i;
                }
            }
        }

        if (closestIndex != -1)
        {
            return bounds.Center + smallestDelta;
        }

        return bounds.Center;
    }

    private ISheetElement? PointOverSelection(Unit2D point)
    {
        foreach (var selected in _sheet.Selection)
        {
            if (_context.SheetRenderer.TryGetElementRenderer(selected, out var renderer) &&
                renderer.HitTest(point))
            {
                return selected;
            }
        }

        return null;
    }

    private UnitBounds GetElementBounds(ISheetElement element)
    {
        if (_context.SheetRenderer.TryGetElementRenderer(element, out var renderer))
        {
            return renderer.SelectionBounds;
        }

        return UnitBounds.Empty;
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
                    if (_context.SheetRenderer.TryGetElementRenderer(element, out var renderer))
                    {
                        renderer.RendererDirty += ForceRedraw;
                    }
                }
            }
        }
        
        ForceRedraw();
    }
    
    public bool CanUnitSnapTo(ISheetElement element)
    {
        return !_sheet.Selection.Contains(element);
    }
    
    public bool CanUnitSnapTo(Handle handle)
    {
        return true;
    }

    private void ForceRedraw()
    {
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        
        foreach (var selected in _sheet.Selection)
        {
            if (_context.SheetRenderer.TryGetElementRenderer(selected, out var renderer))
            {
                var selectionBounds = renderer.SelectionBounds;
                var bounds = new Rect(_context.Viewport.ToPoint(selectionBounds.Min),
                                      _context.Viewport.ToPoint(selectionBounds.Max));

                Pen pen = (selected is ElementGroup) ? _groupPen : _elementPen;
                Brush? fill = null;

                if (_dragState.DraggedElement == selected)
                {
                    fill = (selected is ElementGroup) ? _groupFill : _elementFill;
                }

                dc.DrawRectangle(fill, pen, bounds);

                // dc.DrawRectangle(null, pen, new Rect(bounds.BottomRight, new Size(12, 12)));
                // dc.DrawEllipse(null, pen, bounds.TopRight + new Vector(6, -6), 6, 6);
            }
        }
    }
}
