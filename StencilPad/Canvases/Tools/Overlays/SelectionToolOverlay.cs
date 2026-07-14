using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Widgets;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Spatial;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StencilPad.Canvases.Tools.Overlays;

public class SelectionToolOverlay : FrameworkElement, IUnitSnapContext, IDisposable
{
    public IViewport Viewport => _viewport;

    private readonly ISettings _settings;
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly SheetResolver _sheetResolver;
    private readonly Sheet _sheet;

    private readonly DragState<ISheetElement> _dragState;
    private readonly LockAxisState _lockAxisState;
    private readonly DragState<ISheetElement> _resizeDragState;
    private readonly DragState<ISheetElement> _rotateDragState;

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
    public event Action<Unit2D, Unit2D>? SelectionDragged;
    public event Action? SelectionDragEnded;
    
    public event Action? SelectionResizeStarted;
    public event Action<ISheetElement, Unit2D>? SelectionResized;
    public event Action? SelectionResizeEnded;
    
    public event Action? SelectionRotateStarted;
    public event Action<double, double>? SelectionRotated;
    public event Action? SelectionRotateEnded;
    
    public event Action<ISheetElementAction>? ActionInvoked;

    public SelectionToolOverlay(ISettings settings,
                                IViewport viewport,
                                IUnitSnap unitSnap,
                                Sheet sheet,
                                SheetResolver sheetResolver,
                                SheetElementActionSet actionSet)
    {
        _settings = settings;
        _viewport = viewport;
        _unitSnap = unitSnap;
        _sheetResolver = sheetResolver;
        _sheet = sheet;
        _dragState = new();
        _lockAxisState = new();
        _resizeDragState = new();
        _rotateDragState = new();

        BuildPens();

        BuildInputBindings(actionSet);
        
        ContextMenu = new ContextMenu();
        ContextMenuOpening += (_, e) =>
        {
            if (!BuildContextMenu(actionSet))
            {
                e.Handled = true;
            }
        };
        
        CommandBindings.Add(new CommandBinding(
                                GlobalCommands.SelectAll,
                                (_, _) => SelectAll(),
                                (_, e) => e.CanExecute = true));
        
        CommandBindings.Add(new CommandBinding(
                                GlobalCommands.ClearSelection,
                                (_, _) => ClearSelection(),
                                (_, e) => e.CanExecute = _sheet.Selection.Count > 0));

        _sheetResolver.SelectionAdded += OnSelectionAdded;
        _sheetResolver.SelectionRemoved += OnSelectionRemoved;

        foreach (var resolver in _sheetResolver.Selection)
        {
            OnSelectionAdded(resolver);
        }

        _settings.Changed += SettingsChanged;
    }

    public void Dispose()
    {
        _settings.Changed -= SettingsChanged;
        
        _sheetResolver.SelectionAdded -= OnSelectionAdded;
        _sheetResolver.SelectionRemoved -= OnSelectionRemoved;

        foreach (var resolver in _sheetResolver.Selection)
        {
            OnSelectionRemoved(resolver);
        }
    }

    private void BuildInputBindings(SheetElementActionSet actionSet)
    {
        var builder = new InputBindingsBuilder(_sheet, ActionInvoked, InputBindings);

        builder.Add(Key.P,
                    ModifierKeys.Control,
                    actionSet.ShapeProperties,
                    actionSet.MarkerPathProperties,
                    actionSet.TextProperties,
                    actionSet.RulerProperties,
                    actionSet.ImageProperties);

        builder.Add(Key.C,
                    ModifierKeys.Control | ModifierKeys.Shift,
                    actionSet.CombineShapes);

        builder.Add(Key.G,
                    ModifierKeys.Control,
                    actionSet.Group);

        builder.Add(Key.U,
                    ModifierKeys.Control,
                    actionSet.Ungroup);

        builder.Add(Key.H,
                    ModifierKeys.Control | ModifierKeys.Shift,
                    actionSet.FlipHorizontal);

        builder.Add(Key.V,
                    ModifierKeys.Control | ModifierKeys.Shift,
                    actionSet.FlipVertical);

        builder.Add(Key.F,
                    ModifierKeys.Control,
                    actionSet.BringToFront);

        builder.Add(Key.B,
                    ModifierKeys.Control,
                    actionSet.SendToBack);
    }

    private RelayCommand CreateActionCommand(params ISheetElementAction [] actionSet)
    {
        return new RelayCommand(() =>
        {
            foreach (var action in actionSet)
            {
                if (action.IsEnabled(_sheet, _sheet.Selection) &&
                    action.IsVisible(_sheet, _sheet.Selection))
                {
                    action.Invoke(_sheet, _sheet.Selection);
                    return;
                }
            }
        });
    }

    private bool BuildContextMenu(SheetElementActionSet actionSet)
    {
        if (_sheet.Selection.Count == 0)
        {
            return false;
        }

        ContextMenu.Items.Clear();
        
        var builder = new ContextMenuBuilder(_sheet, ActionInvoked);

        if (builder.AddContextMenuItemSet(
                ContextMenu.Items,
                (actionSet.ShapeProperties, "Shape Properties…", "Ctrl+P"),
                (actionSet.MarkerPathProperties, "Marker Path Properties…", "Ctrl+P"),
                (actionSet.TextProperties, "Text Properties…", "Ctrl+P"),
                (actionSet.RulerProperties, "Ruler Properties…", "Ctrl+P"),
                (actionSet.ImageProperties, "Image Properties…", "Ctrl+P"),
                (actionSet.CombineShapes, "Combine Shapes", "Ctrl+Shift+C")))
        {
            ContextMenu.Items.Add(new Separator());
        }

        builder.AddContextMenuItemSet(
            ContextMenu.Items,
            (actionSet.Group, "Group", "Ctrl+G"),
            (actionSet.Ungroup, "Ungroup", "Ctrl+U"));
        
        ContextMenu.Items.Add(new Separator());

        builder.AddContextMenuItemSet(
            ContextMenu.Items,
            (actionSet.FlipHorizontal, "Flip Horizontal", "Ctrl+Shift+H"),
            (actionSet.FlipVertical, "Flip Vertical", "Ctrl+Shift+V"));

        ContextMenu.Items.Add(new Separator());

        var justifyGroup = new MenuItem { Header = "Justify" };

        ContextMenu.Items.Add(justifyGroup);

        builder.AddContextMenuItemSet(
            justifyGroup.Items,
            (actionSet.JustifyLeft, "Left", ""),
            (actionSet.JustifyCenter, "Centre", ""),
            (actionSet.JustifyRight, "Right", ""));

        justifyGroup.Items.Add(new Separator());
        
        builder.AddContextMenuItemSet(
            justifyGroup.Items,
            (actionSet.JustifyTop, "Top", ""),
            (actionSet.JustifyMiddle, "Middle", ""),
            (actionSet.JustifyBottom, "Bottom", ""));
        
        ContextMenu.Items.Add(new Separator());

        builder.AddContextMenuItemSet(
            ContextMenu.Items,
            (actionSet.BringToFront, "Bring to Front", "Ctrl+F"),
            (actionSet.SendToBack, "Send to Back", "Ctrl+B"));

        return true;
    }
    
    private void BuildPens()
    {
        var selectionColor = _settings.SelectionColor;
        var groupSelectionColor = _settings.GroupSelectionColor;

        _elementPen = new Pen(new SolidColorBrush(ColorUtil.WithAlpha(selectionColor, 128)), 2);
        _elementPen.Freeze();

        _elementFill = new SolidColorBrush(ColorUtil.WithAlpha(selectionColor, 32));
        _elementFill.Freeze();

        _groupPen = new Pen(new SolidColorBrush(ColorUtil.WithAlpha(groupSelectionColor, 128)), 2);
        _groupPen.Freeze();

        _groupFill = new SolidColorBrush(ColorUtil.WithAlpha(groupSelectionColor, 32));
        _groupFill.Freeze();

        _resizeHandleSize = _settings.HandleSizePx;
        _rotateHandleRadius = _settings.HandleSizePx / 2;
    }
    
    private void SettingsChanged()
    {
        BuildPens();
        InvalidateVisual();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        var mousePosition = e.GetPosition(this);

        foreach (var resolver in _sheetResolver.Selection)
        {
            var element = resolver.Element;
            var unitBounds = element.GetBounds();
            var screenBounds = _viewport.ToRect(resolver.GetOutlineBounds());
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
            var elementBounds = elementUnderMouse.GetBounds();

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

                // Clamp target to not go past the initial NW corner - this
                // avoids both destroying all information about an object by
                // setting all it's points to converge on a single point, and
                // also avoids technically valid but weird and unexpected
                // behaviour where we resize in reverse across the NW corner.
                
                targetSE = new Unit2D(Unit.Max(targetSE.X, _resizeInitialNW.X + Unit.FromMillimeters(0.1)),
                                      Unit.Min(targetSE.Y, _resizeInitialNW.Y - Unit.FromMillimeters(0.1)));

                var size = Unit2D.Abs(targetSE - _resizeInitialNW);

                SelectionResized?.Invoke(_resizeDragState.DraggedElement, targetSE - _resizeInitialSE);
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

                if (ModifierUtil.IsAngleSnap())
                {
                    var snapAngle = _settings.AngleSnapDegrees * MathUtil.Deg2Rad;
                    
                    totalAngle = Math.Round(totalAngle / snapAngle) * snapAngle;
                }
                
                var angleDelta = totalAngle - _lastRotateAngle;
                
                _lastRotateAngle = totalAngle;

                SelectionRotated?.Invoke(totalAngle, angleDelta);
                e.Handled = true;
            }

            return;
        }

        if (_dragState.DragStarted)
        {
            var elementBounds = _dragState.DraggedElement.GetBounds();
            var result = _dragState.OnDragMove(_viewport, mousePosition);

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
                var totalDelta = snappedCenter - _dragState.InitialElementPosition;
                
                SelectionDragged?.Invoke(totalDelta, delta);
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
        Unit2D smallestDelta = Unit2D.FromSquare(Unit.FromMillimeters(1000));

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
        foreach (var resolver in _sheetResolver.Selection)
        {
            if (resolver.OutlineContainsPoint(point))
            {
                return resolver.Element;
            }
        }

        return null;
    }

    private void OnSelectionAdded(ISheetElementResolver resolver)
    {
        resolver.OutlineChanged += ForceRedraw;

        ForceRedraw();
    }

    private void OnSelectionRemoved(ISheetElementResolver resolver)
    {
        resolver.OutlineChanged -= ForceRedraw;

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
        var dy = _resizeInitialNW.Y - targetSE.Y;

        var seAy = dx / _resizeAspectRatio;
        var seBx = dy * _resizeAspectRatio;

        if (Unit.Abs(seAy - dy) <= Unit.Abs(seBx - dx))
        {
            return new Unit2D(targetSE.X, _resizeInitialNW.Y - seAy);
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

        foreach (var resolver in _sheetResolver.Selection)
        {
            var screenBounds = _viewport.ToRect(resolver.GetOutlineBounds());
            var element = resolver.Element;

            Pen pen = (element is ElementGroup) ? _groupPen : _elementPen;
            Brush? fill = null;

            if (_dragState.DraggedElement == element ||
                _rotateDragState.DraggedElement == element ||
                _resizeDragState.DraggedElement == element)
            {
                fill = (element is ElementGroup) ? _groupFill : _elementFill;
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
