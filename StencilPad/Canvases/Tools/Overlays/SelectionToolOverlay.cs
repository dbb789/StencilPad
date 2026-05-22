using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Widgets;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class SelectionToolOverlay : FrameworkElement, IUnitSnapContext, IGlobalCommandTarget, IDisposable
{
    private const double ResizeHandleSize = 12;
    private const double RotateHandleRadius = 6;

    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly Sheet _sheet;

    private DragState<ISheetElement> _dragState;
    private DragState<bool> _resizeDragState;
    private DragState<bool> _rotateDragState;

    private Unit2D _resizeInitialSE;
    private Unit2D _rotateInitialHandlePos;
    private Unit2D _rotateDragCenter;
    private double _lastRotateAngle;

    private Pen _elementPen;
    private Brush _elementFill;
    private Pen _groupPen;
    private Brush _groupFill;
    private Pen _handlePen;

    public event Action<Unit2D>? SelectionDragged;
    public event Action? SelectionResizeStarted;
    public event Action<Unit2D>? SelectionResized;
    public event Action? SelectionRotateStarted;
    public event Action<double>? SelectionRotated;
    public event Action<ISheetElementAction>? ActionInvoked;

    public SelectionToolOverlay(IViewport viewport,
                                IUnitSnap unitSnap,
                                Sheet sheet,
                                SheetElementActionSet actionSet)
    {
        _viewport = viewport;
        _unitSnap = unitSnap;
        _sheet = sheet;
        _sheet.Selection.CollectionChanged += SelectionChanged;
        _dragState = new();
        _resizeDragState = new();
        _rotateDragState = new();

        _elementPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)), 2);
        _elementPen.Freeze();

        _elementFill = new SolidColorBrush(Color.FromArgb(10, 0, 0, 255));
        _elementFill.Freeze();

        _groupPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 128, 255)), 2);
        _groupPen.Freeze();

        _groupFill = new SolidColorBrush(Color.FromArgb(10, 0, 128, 255));
        _groupFill.Freeze();

        _handlePen = new Pen(new SolidColorBrush(Color.FromArgb(200, 0, 0, 200)), 1.5);
        _handlePen.Freeze();

        ContextMenu = new ContextMenu();
        ContextMenuOpening += (s, e) => RebuildContextMenu(s, e, actionSet.Actions);


        foreach (var element in _sheet.Selection)
        {
            element.TransformChanged += OnTransformChanged;
            element.GeometryChanged += OnTransformChanged;
        }
    }

    public void Dispose()
    {
        _sheet.Selection.CollectionChanged -= SelectionChanged;

        foreach (var element in _sheet.Selection)
        {
            element.TransformChanged -= OnTransformChanged;
            element.GeometryChanged -= OnTransformChanged;
        }
    }

    private void RebuildContextMenu(object sender,
                                    ContextMenuEventArgs e,
                                    IEnumerable<ISheetElementAction?> actions)
    {
        if (!ContextMenuUtil.RebuildContextMenu(ContextMenu,
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

        foreach (var element in _sheet.Selection)
        {
            var bounds = element.GetTransformedBounds();
            var screenBounds = new Rect(_viewport.ToPoint(bounds.Min),
                                        _viewport.ToPoint(bounds.Max));

            var resizeRect = new Rect(screenBounds.BottomRight,
                                      new Size(ResizeHandleSize, ResizeHandleSize));

            if (resizeRect.Contains(mousePosition))
            {
                _resizeInitialSE = bounds.SE;
                _resizeDragState.OnDragStart(mousePosition, true, _resizeInitialSE);

                SelectionResizeStarted?.Invoke();

                CaptureMouse();
                e.Handled = true;
                return;
            }

            var rotateCenter = screenBounds.TopRight + new Vector(RotateHandleRadius, -RotateHandleRadius);
            var dx = mousePosition.X - rotateCenter.X;
            var dy = mousePosition.Y - rotateCenter.Y;

            if (dx * dx + dy * dy <= RotateHandleRadius * RotateHandleRadius)
            {
                _rotateDragCenter = bounds.Center;
                _rotateInitialHandlePos = _viewport.FromPoint(rotateCenter);
                _lastRotateAngle = 0;
                _rotateDragState.OnDragStart(mousePosition, true, _rotateInitialHandlePos);

                SelectionRotateStarted?.Invoke();

                CaptureMouse();
                e.Handled = true;
                return;
            }
        }

        var elementUnderMouse = PointOverSelection(_viewport.FromPoint(mousePosition));

        if (elementUnderMouse != null)
        {
            var elementBounds = elementUnderMouse.GetTransformedBounds();

            _dragState.OnDragStart(mousePosition,
                                   elementUnderMouse,
                                   elementBounds.Center);

            CaptureMouse();
            e.Handled = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var mousePosition = e.GetPosition(this);

        if (_resizeDragState.DragStarted)
        {
            var result = _resizeDragState.OnDragMove(_viewport, mousePosition);

            if (result is not null)
            {
                var targetSE = _unitSnap.UnitSnap(result.Value.TargetElementPosition, this)
                               ?? result.Value.TargetElementPosition;
                SelectionResized?.Invoke(targetSE - _resizeInitialSE);
                e.Handled = true;
            }

            return;
        }

        if (_rotateDragState.DragStarted)
        {
            var result = _rotateDragState.OnDragMove(_viewport, mousePosition);

            if (result is not null)
            {
                var initialVec = _rotateInitialHandlePos - _rotateDragCenter;
                var currentVec = result.Value.TargetElementPosition - _rotateDragCenter;
                var totalAngle = Unit2D.SignedAngle(initialVec, currentVec);
                var angleDelta = totalAngle - _lastRotateAngle;
                _lastRotateAngle = totalAngle;
                SelectionRotated?.Invoke(angleDelta);
                e.Handled = true;
            }

            return;
        }

        if (!_dragState.DragStarted)
        {
            return;
        }

        var elementBounds = _dragState.DraggedElement.GetTransformedBounds();
        var dragResult = _dragState.OnDragMove(_viewport,
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
        _resizeDragState.OnDragEnd();
        _rotateDragState.OnDragEnd();

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
            var snapPosition = _unitSnap.UnitSnap(corners[i], this);

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
            if (selected.GetTransformedBounds().Contains(point))
            {
                return selected;
            }
        }

        return null;
    }

    private void SelectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ISheetElement element in e.NewItems)
            {
                element.TransformChanged += OnTransformChanged;
                element.GeometryChanged += OnTransformChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (ISheetElement element in e.OldItems)
            {
                element.TransformChanged -= OnTransformChanged;
                element.GeometryChanged -= OnTransformChanged;
            }
        }

        ForceRedraw();
    }

    private void OnTransformChanged(ISheetElement element)
    {
        ForceRedraw();
    }

    public void SelectAll()
    {
        _sheet.Selection.Clear();

        foreach (var element in _sheet.Elements)
        {
            _sheet.Selection.Add(element);
        }
    }

    public void ClearSelection()
    {
        _sheet.Selection.Clear();
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
            var unitBounds = selected.GetTransformedBounds();
            var bounds = new Rect(_viewport.ToPoint(unitBounds.Min),
                                  _viewport.ToPoint(unitBounds.Max));

            Pen pen = (selected is ElementGroup) ? _groupPen : _elementPen;
            Brush? fill = null;

            if (_dragState.DraggedElement == selected)
            {
                fill = (selected is ElementGroup) ? _groupFill : _elementFill;
            }

            dc.DrawRectangle(fill, pen, bounds);

            dc.DrawRectangle(null, pen, new Rect(bounds.BottomRight, new Size(ResizeHandleSize, ResizeHandleSize)));
            dc.DrawEllipse(null, pen, bounds.TopRight + new Vector(RotateHandleRadius, -RotateHandleRadius), RotateHandleRadius, RotateHandleRadius);
        }
    }
}
