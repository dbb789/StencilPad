using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Controllers.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Widgets;
using StencilPad.Models;
using StencilPad.Rendering;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class EditToolOverlay : Canvas, IUnitSnapContext, IDisposable
{
    public class Factory(Sheet Sheet,
                         IViewport Viewport,
                         IHandleMap HandleMap,
                         IEditOverlayRenderer EditOverlayRenderer,
                         IUnitSnap UnitSnap,
                         IUnitSnapOverlay UnitSnapOverlay,
                         PolygonSheetElementEditActionSet ActionSet)
    {
        public EditToolOverlay Create()
        {
            return new EditToolOverlay(Sheet,
                                       Viewport,
                                       HandleMap,
                                       EditOverlayRenderer,
                                       UnitSnap,
                                       UnitSnapOverlay,
                                       ActionSet);
        }
    }
    
    // Limit mouse move event handling to 60hz so we don't clog up WPF.
    private const long MouseMoveEventThrottleMs = 16;
    
    private record struct HandleEntry(ISheetElement Element, Handle Handle);
    private record struct WidgetEntry(ISheetElement Element, HandleWidget Widget);
    
    public event Action<ISheetElement, Handle>? HandleDragBegin;
    public event Action<ISheetElement, Handle, Unit2D>? HandleDragged;
    public event Action? HandleDragEnd;
    public event Action<ISheetElement, Handle>? HandleSelected;
    public event Action<ISheetElementAction>? ActionInvoked;

    private readonly Sheet _sheet;
    private readonly IViewport _viewport;
    private readonly IHandleMap _handleMap;
    private readonly IEditOverlayRenderer _editOverlayRenderer;
    private readonly IUnitSnap _unitSnap;
    private readonly IUnitSnapOverlay _unitSnapOverlay;
    
    private List<IHandleMapEntry> _queryResults;
    private DragState<IHandleMapEntry> _dragState;
    private LockAxisState _lockAxisState;
    
    private long _lastMouseMoveEvent;
    private Brush _moveBrush;
    private Brush _adjustBrush;
    private Pen _selectedPen;
    private Pen _axisLockPen;
    
    private EditToolOverlay(Sheet sheet,
                            IViewport viewport,
                            IHandleMap handleMap,
                            IEditOverlayRenderer editOverlayRenderer,
                            IUnitSnap unitSnap,
                            IUnitSnapOverlay unitSnapOverlay,
                            PolygonSheetElementEditActionSet actionSet)
    {
        _sheet = sheet;
        _viewport = viewport;
        _handleMap = handleMap;
        _editOverlayRenderer = editOverlayRenderer;
        _unitSnap = unitSnap;
        _unitSnapOverlay = unitSnapOverlay;
        
        _queryResults = new(128);
        _dragState = new();
        _lockAxisState = new();
        
        _viewport.ViewportChanged += ForceRedraw;
        _handleMap.SheetSelectionChanged += ForceRedraw;
        _handleMap.HandleAdded += OnHandleAdded;
        _handleMap.HandleRemoved += OnHandleRemoved;
        _handleMap.HandleMoved += OnHandleMoved;
        _handleMap.HandleSelectionChanged += ForceRedraw;

        _moveBrush = new SolidColorBrush(Color.FromArgb(128, 255, 128, 0));
        _moveBrush.Freeze();

        _adjustBrush = new SolidColorBrush(Color.FromArgb(128, 0, 128, 0));
        _adjustBrush.Freeze();

        _selectedPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)), 2);
        _selectedPen.Freeze();

        _axisLockPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)), 1);
        _axisLockPen.Freeze();
        
        ContextMenu = new ContextMenu();
        ContextMenuOpening += (s, e) => RebuildContextMenu(s, e, actionSet.Actions);
        
        _editOverlayRenderer.IsEnabled = true;
    }

    public void Dispose()
    {
        _editOverlayRenderer.IsEnabled = false;
        _viewport.ViewportChanged -= ForceRedraw;

        _handleMap.SheetSelectionChanged -= ForceRedraw;
        _handleMap.HandleAdded -= OnHandleAdded;
        _handleMap.HandleRemoved -= OnHandleRemoved;
        _handleMap.HandleMoved -= OnHandleMoved;
        _handleMap.HandleSelectionChanged += ForceRedraw;
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
        var mousePosition = e.GetPosition(VisualTreeHelper.GetParent(this) as UIElement);

        var clickPosition = _viewport.FromPoint(mousePosition);
        var clickSizeUnit = _viewport.FromPixels(16);
        var clickSize = new Unit2D(clickSizeUnit, clickSizeUnit);

        var handle = _handleMap.GetClosestHandle(UnitBounds.FromCenterSize(clickPosition, clickSize));

        if (handle is null || !handle.Editing)
        {
            return;
        }
        
        _dragState.OnDragStart(mousePosition,
                               handle,
                               handle.Position);
        _lockAxisState.OnDragStart();
        
        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (_dragState.IsDragging)
        {
            HandleDragEnd?.Invoke();
            
            _unitSnapOverlay.End();
        }
        else if (_dragState.DraggedElement is not null)
        {
            HandleSelected?.Invoke(_dragState.DraggedElement.Element,
                                   _dragState.DraggedElement.Handle);
        }

        _dragState.OnDragEnd();
        _lockAxisState.OnDragEnd();

        ForceRedraw();

        ReleaseMouseCapture();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var now = Environment.TickCount;
        
        if (_lastMouseMoveEvent > (now - MouseMoveEventThrottleMs))
        {
            return;
        }
        
        _lastMouseMoveEvent = now;
        
        var mousePosition = e.GetPosition(VisualTreeHelper.GetParent(this) as UIElement);

        if (!_dragState.DragStarted)
        {
            return;
        }
        
        var dragResult = _dragState.OnDragMove(_viewport,
                                               mousePosition);

        if (dragResult is null)
        {
            return;
        }

        if (dragResult.Value.IsDragBeginning)
        {
            HandleDragBegin?.Invoke(_dragState.DraggedElement.Element,
                                    _dragState.DraggedElement.Handle);

            _unitSnapOverlay.Begin(this);
        }

        var snappedTarget = _unitSnap.UnitSnap(dragResult.Value.TargetElementPosition, this);
        var targetPosition = snappedTarget ?? dragResult.Value.TargetElementPosition;
        
        targetPosition = _lockAxisState.OnDragMove(ModifierUtil.IsLockToAxis(),
                                                   _viewport.FromPixels(12),
                                                   _dragState.InitialElementPosition,
                                                   targetPosition);

        var delta = targetPosition - _dragState.DraggedElement.Position;

        HandleDragged?.Invoke(_dragState.DraggedElement.Element,
                              _dragState.DraggedElement.Handle,
                              delta);

        e.Handled = true;
    }

    private void OnHandleAdded(ISheetElement element, Handle handle, Unit2D position)
    {
        ForceRedraw();
    }

    private void OnHandleRemoved(ISheetElement element, Handle handle)
    {
        ForceRedraw();
    }

    private void OnHandleMoved(ISheetElement element, Handle handle, Unit2D position)
    {
        ForceRedraw();
    }

    public bool CanUnitSnapTo(ISheetElement element)
    {
        return true;
    }
    
    public bool CanUnitSnapTo(Handle handle)
    {
        if (_handleMap.TryGetHandleEntry(handle, out var entry))
        {
            return !entry.Selected;
        }

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

        _queryResults.Clear();
        _handleMap.QueryHandles(UnitBounds.FromCenterSize(Unit2D.Zero,
                                                                  _viewport.Size),
                                        _queryResults);
        
        foreach (var entry in _queryResults)
        {
            if (!entry.Editing)
            {
                continue;
            }
            
            var point = _viewport.ToPoint(entry.Position);
            var pen = entry.Selected ? _selectedPen : null;
           
            if (entry.Handle.Type == HandleType.Move)
            {
                dc.DrawRectangle(_moveBrush, pen, new Rect(point.X - 6, point.Y - 6, 12, 12));
            }
            else
            {
                dc.DrawEllipse(_adjustBrush, pen, point, 6, 6);
            }
        }

        if (_lockAxisState.LockedAxis is not null && _lockAxisState.LockPosition is not null)
        {
            if (_lockAxisState.LockedAxis == UnitAxis.X)
            {
                var lockPoint = _viewport.ToPoint(new Unit2D(Unit.Zero, _lockAxisState.LockPosition.Value));
                
                dc.DrawLine(_axisLockPen,
                            new Point(0, lockPoint.Y),
                            new Point(RenderSize.Width, lockPoint.Y));
            }
            else
            {
                var lockPoint = _viewport.ToPoint(new Unit2D(_lockAxisState.LockPosition.Value, Unit.Zero));

                dc.DrawLine(_axisLockPen,
                            new Point(lockPoint.X, 0),
                            new Point(lockPoint.X, RenderSize.Height));
            }
        }
    }
}
