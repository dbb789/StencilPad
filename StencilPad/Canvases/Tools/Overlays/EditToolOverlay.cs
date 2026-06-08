using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Widgets;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Spatial;
using StencilPad.Services;

namespace StencilPad.Canvases.Tools.Overlays;

public class EditToolOverlay : ToolOverlay, IUnitSnapContext, IGlobalCommandTarget, IDisposable
{
    // Limit mouse move event handling to 60hz so we don't clog up WPF.
    private const long MouseMoveEventThrottleMs = 16;
    private record struct HandleEntry(ISheetElement Element, Handle Handle);
    private record struct WidgetEntry(ISheetElement Element, HandleWidget Widget);
    
    public IViewport Viewport => _viewport;
    
    public event Action<ISheetElement, Handle>? HandleDragBegin;
    public event Action<ISheetElement, Handle, Unit2D>? HandleDragged;
    public event Action? HandleDragEnd;
    public event Action<ISheetElement, Handle>? HandleSelected;
    public event Action<ISheetElementAction>? ActionInvoked;

    private readonly Sheet _sheet;
    private readonly IAppConfigService _appConfigService;
    private readonly IViewport _viewport;
    private readonly IHandleMap _handleMap;
    private readonly IUnitSnap _unitSnap;
    private readonly IUnitSnapOverlay _unitSnapOverlay;
    
    private List<IHandleMapEntry> _queryResults;
    private DragState<IHandleMapEntry> _dragState;
    private LockAxisState _lockAxisState;
    private long _lastMouseMoveEvent;
    
    private double _handleSize;
    private Brush _moveBrush = null!;
    private Brush _adjustBrush = null!;
    private Pen _selectedPen = null!;
    private Pen _axisLockPen = null!;
    
    public EditToolOverlay(Sheet sheet,
                           IAppConfigService appConfigService,
                           IViewport viewport,
                           IHandleMap handleMap,
                           IUnitSnap unitSnap,
                           IUnitSnapOverlay unitSnapOverlay,
                           SheetElementEditActionSet actionSet)
        : base(viewport, sheet, true)
    {
        _sheet = sheet;
        _appConfigService = appConfigService;
        _viewport = viewport;
        _handleMap = handleMap;
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

        BuildPens();
        
        RegisterOverlay(PolygonToolOverlayRenderer.Factory);
        RegisterOverlay(TextElementToolOverlayRenderer.Factory);
        RegisterOverlay(ImageElementToolOverlayRenderer.Factory);

        ContextMenu = new ContextMenu();
        ContextMenuOpening += (s, e) => RebuildContextMenu(s, e, actionSet.Actions);
        
        _appConfigService.ConfigChanged += OnConfigChanged;
    }

    public override void Dispose()
    {
        _appConfigService.ConfigChanged -= OnConfigChanged;
                
        _viewport.ViewportChanged -= ForceRedraw;

        _handleMap.SheetSelectionChanged -= ForceRedraw;
        _handleMap.HandleAdded -= OnHandleAdded;
        _handleMap.HandleRemoved -= OnHandleRemoved;
        _handleMap.HandleMoved -= OnHandleMoved;
        _handleMap.HandleSelectionChanged -= ForceRedraw;

        base.Dispose();
    }

    private void BuildPens()
    {
        var config = _appConfigService.Config;
        var moveHandleColor = config.MoveHandleColor;
        var adjustHandleColor = config.AdjustHandleColor;
        var selectionColor = config.SelectionColor;
        var gridLineColor = config.GridLineColor;
        
        _moveBrush = new SolidColorBrush(ColorUtil.WithAlpha(moveHandleColor, 128));
        _moveBrush.Freeze();

        _adjustBrush = new SolidColorBrush(ColorUtil.WithAlpha(adjustHandleColor, 128));
        _adjustBrush.Freeze();

        _selectedPen = new Pen(new SolidColorBrush(ColorUtil.WithAlpha(selectionColor, 255)), 2);
        _selectedPen.Freeze();

        _axisLockPen = new Pen(new SolidColorBrush(ColorUtil.WithAlpha(gridLineColor, 128)), 2);
        _axisLockPen.Freeze();

        _handleSize = config.HandleSizePx;
    }
    
    private void OnConfigChanged()
    {
        BuildPens();
        InvalidateVisual();
    }

    public void SelectAll()
    {
        _handleMap.SelectAll();
    }
    
    public void ClearSelection()
    {
        _handleMap.ClearSelection();
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
        var clickSizeUnit = _viewport.FromPixels(_handleSize + 4);
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
                                                   _viewport.FromPixels(_handleSize),
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

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        RenderOverlay(dc);

        _queryResults.Clear();
        _handleMap.QueryHandles(UnitBounds.FromCenterSize(Unit2D.Zero, _viewport.Size),
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
                dc.DrawRectangle(_moveBrush,
                                 pen,
                                 new Rect(point.X - (_handleSize / 2),
                                          point.Y - (_handleSize / 2),
                                          _handleSize,
                                          _handleSize));
            }
            else
            {
                dc.DrawEllipse(_adjustBrush,
                               pen,
                               point,
                               _handleSize / 2,
                               _handleSize / 2);
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
