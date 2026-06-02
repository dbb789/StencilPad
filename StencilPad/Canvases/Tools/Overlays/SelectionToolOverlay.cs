using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Common;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Widgets;
using StencilPad.Models;
using StencilPad.Spatial;
using StencilPad.Services;

namespace StencilPad.Canvases.Tools.Overlays;

public class SelectionToolOverlay : FrameworkElement, IUnitSnapContext, IGlobalCommandTarget, IDisposable
{
    private readonly IAppConfigService _appConfigService;
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly Sheet _sheet;

    private DragState<ISheetElement> _dragState;
    private LockAxisState _lockAxisState;
    private DragState<ISheetElement> _resizeDragState;
    private DragState<ISheetElement> _rotateDragState;

    private Unit2D _resizeInitialNW;
    private Unit2D _resizeInitialSE;
    private double _resizeAspectRatio;
    private Unit2D _rotateInitialHandlePos;
    private Unit2D _rotateDragCenter;
    private double _lastRotateAngle;

    private double _resizeHandleSize;
    private double _rotateHandleRadius;
    private Pen _elementPen = null!;
    private Brush _elementFill = null!;
    private Pen _groupPen = null!;
    private Brush _groupFill = null!;

    public event Action? SelectionDragStarted;
    public event Action<Unit2D>? SelectionDragged;
    public event Action? SelectionDragEnded;
    
    public event Action? SelectionResizeStarted;
    public event Action<Unit2D>? SelectionResized;
    public event Action? SelectionResizeEnded;
    
    public event Action? SelectionRotateStarted;
    public event Action<double>? SelectionRotated;
    public event Action? SelectionRotateEnded;
    
    public event Action<ISheetElementAction>? ActionInvoked;

    public SelectionToolOverlay(IAppConfigService appConfigService,
                                IViewport viewport,
                                IUnitSnap unitSnap,
                                Sheet sheet,
                                SheetElementActionSet actionSet)
    {
        _appConfigService = appConfigService;
        _viewport = viewport;
        _unitSnap = unitSnap;
        _sheet = sheet;
        _sheet.Selection.CollectionChanged += SelectionChanged;
        _dragState = new();
        _lockAxisState = new();
        _resizeDragState = new();
        _rotateDragState = new();

        BuildPens();
        
        ContextMenu = new ContextMenu();
        ContextMenuOpening += (s, e) => RebuildContextMenu(s, e, actionSet.Actions);

        foreach (var element in _sheet.Selection)
        {
            element.TransformChanged += OnTransformChanged;
            element.GeometryChanged += OnTransformChanged;
        }

        _appConfigService.ConfigChanged += OnConfigChanged;
    }
    
    public void Dispose()
    {
        _appConfigService.ConfigChanged -= OnConfigChanged;
        
        _sheet.Selection.CollectionChanged -= SelectionChanged;

        foreach (var element in _sheet.Selection)
        {
            element.TransformChanged -= OnTransformChanged;
            element.GeometryChanged -= OnTransformChanged;
        }
    }

    private void BuildPens()
    {
        var config = _appConfigService.Config;
        var selectionColor = config.SelectionColor;
        var groupSelectionColor = config.GroupSelectionColor;

        _elementPen = new Pen(new SolidColorBrush(ColorUtil.WithAlpha(selectionColor, 128)), 2);
        _elementPen.Freeze();

        _elementFill = new SolidColorBrush(ColorUtil.WithAlpha(selectionColor, 10));
        _elementFill.Freeze();

        _groupPen = new Pen(new SolidColorBrush(ColorUtil.WithAlpha(groupSelectionColor, 128)), 2);
        _groupPen.Freeze();

        _groupFill = new SolidColorBrush(ColorUtil.WithAlpha(groupSelectionColor, 10));
        _groupFill.Freeze();

        _resizeHandleSize = config.HandleSizePx;
        _rotateHandleRadius = config.HandleSizePx / 2;
    }
    
    private void OnConfigChanged()
    {
        BuildPens();
        InvalidateVisual();
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
            var unitBounds = element.GetTransformedBounds();
            var screenBounds = _viewport.ToRect(unitBounds);
            var resizeRect = ResizeHandleRect(screenBounds);

            if (resizeRect.Contains(mousePosition))
            {
                _resizeInitialNW = unitBounds.NW;
                _resizeInitialSE = unitBounds.SE;
                _resizeAspectRatio = unitBounds.Size.X.Millimeters / unitBounds.Size.Y.Millimeters;
                _resizeDragState.OnDragStart(mousePosition, element, _resizeInitialSE);

                CaptureMouse();
                e.Handled = true;
                return;
            }

            var rotateRect = RotateHandleRect(screenBounds);

            if (rotateRect.Contains(mousePosition))
            {
                _rotateDragCenter = unitBounds.Center;
                _rotateInitialHandlePos = _viewport.FromPoint(mousePosition);
                _lastRotateAngle = 0;
                _rotateDragState.OnDragStart(mousePosition, element, _rotateInitialHandlePos);

                CaptureMouse();
                e.Handled = true;
                return;
            }
        }

        var elementUnderMouse = PointOverSelection(_viewport.FromPoint(mousePosition));

        if (elementUnderMouse is not null)
        {
            var elementBounds = elementUnderMouse.GetTransformedBounds();

            _dragState.OnDragStart(mousePosition,
                                   elementUnderMouse,
                                   elementBounds.Center);
            _lockAxisState.OnDragStart();
            
            CaptureMouse();
            e.Handled = true;
            return;
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
                if (result.Value.IsDragBeginning)
                {
                    SelectionResizeStarted?.Invoke();
                }
                
                var targetSE = _unitSnap.UnitSnap(result.Value.TargetElementPosition, this)
                               ?? result.Value.TargetElementPosition;

                if (ModifierUtil.IsLockAspect())
                {
                    targetSE = LockAspect(targetSE);
                }

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
                if (result.Value.IsDragBeginning)
                {
                    SelectionRotateStarted?.Invoke();
                }

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

        if (_dragState.DragStarted)
        {
            var elementBounds = _dragState.DraggedElement.GetTransformedBounds();
            var result = _dragState.OnDragMove(_viewport,
                                                   mousePosition);

            if (result is not null)
            {
                if (result.Value.IsDragBeginning)
                {
                    SelectionDragStarted?.Invoke();
                }
                
                var targetPosition = result.Value.TargetElementPosition;
                var targetBounds = UnitBounds.FromCenterSize(targetPosition, elementBounds.Size);
                var snappedCenter = SnapBoundsCenter(targetBounds);

                snappedCenter = _lockAxisState.OnDragMove(ModifierUtil.IsLockToAxis(),
                                                          _viewport.FromPixels(_resizeHandleSize),
                                                          _dragState.InitialElementPosition,
                                                          snappedCenter);

                var delta = snappedCenter - elementBounds.Center;

                SelectionDragged?.Invoke(delta);
                e.Handled = true;
            }
            
            return;
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        // Capture drag state before we clear everything out.
        bool dragHandled = false;

        if (_dragState.IsDragging)
        {
            dragHandled = true;
            SelectionDragEnded?.Invoke();
        }

        if (_resizeDragState.IsDragging)
        {
            dragHandled = true;
            SelectionResizeEnded?.Invoke();
        }

        if (_rotateDragState.IsDragging)
        {
            dragHandled = true;
            SelectionRotateEnded?.Invoke();
        }

        // Holding down the left button over a draggable item without actually
        // moving the mouse can start the drag state, so make sure these are all
        // cleared out regardless.
        _dragState.OnDragEnd();
        _lockAxisState.OnDragEnd();
        _resizeDragState.OnDragEnd();
        _rotateDragState.OnDragEnd();

        ReleaseMouseCapture();
        e.Handled = dragHandled;

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
            if (selected.ContainsPoint(point))
            {
                return selected;
            }
        }

        return null;
    }

    private void SelectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {       
        if (e.OldItems != null)
        {
            foreach (ISheetElement element in e.OldItems)
            {
                element.TransformChanged -= OnTransformChanged;
                element.GeometryChanged -= OnTransformChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (ISheetElement element in e.NewItems)
            {
                element.TransformChanged += OnTransformChanged;
                element.GeometryChanged += OnTransformChanged;
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

    private Unit2D LockAspect(Unit2D targetSE)
    {
        var dx = targetSE.X - _resizeInitialNW.X;
        var dy = targetSE.Y - _resizeInitialNW.Y;

        var seAy = dx / _resizeAspectRatio;
        var seBx = dy * _resizeAspectRatio;

        if (Unit.Abs(seAy - dy) <= Unit.Abs(seBx - dx))
        {
            return new Unit2D(targetSE.X, _resizeInitialNW.Y + seAy);
        }
        else
        {
            return new Unit2D(_resizeInitialNW.X + seBx, targetSE.Y);
        }
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
            var screenBounds = _viewport.ToRect(unitBounds);

            Pen pen = (selected is ElementGroup) ? _groupPen : _elementPen;
            Brush? fill = null;

            if (_dragState.DraggedElement == selected ||
                _rotateDragState.DraggedElement == selected ||
                _resizeDragState.DraggedElement == selected)
            {
                fill = (selected is ElementGroup) ? _groupFill : _elementFill;
            }
            
            dc.DrawRectangle(fill, pen, screenBounds);

            dc.DrawRectangle(null,
                             pen,
                             ResizeHandleRect(screenBounds));

            var rotateHandleRect = RotateHandleRect(screenBounds);

            dc.DrawEllipse(null,
                           pen,
                           new Point(rotateHandleRect.Left + rotateHandleRect.Width / 2,
                                     rotateHandleRect.Top + rotateHandleRect.Height / 2),
                           rotateHandleRect.Width / 2, rotateHandleRect.Height / 2);
        }
    }

    private Rect RotateHandleRect(Rect screenBounds)
    {
        return new Rect(screenBounds.TopRight + new Vector(0, -_resizeHandleSize),
                        new Size(_resizeHandleSize, _resizeHandleSize));
    }

    private Rect ResizeHandleRect(Rect screenBounds)
    {
        return new Rect(screenBounds.BottomRight,
                        new Size(_resizeHandleSize, _resizeHandleSize));
    }
}
