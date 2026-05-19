using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Widgets;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class EditHandleSetToolOverlay : Canvas, IUnitSnapContext, IDisposable
{
    // Limit mouse move event handling to 60hz so we don't clog up WPF.
    private const long MouseMoveEventThrottleMs = 16;
    
    private record struct HandleEntry(ISheetElement Element, Handle Handle);
    private record struct WidgetEntry(ISheetElement Element, HandleWidget Widget);
    
    public event Action<IHandleSource, Handle>? HandleDragBegin;
    public event Action<IHandleSource, Handle, Unit2D>? HandleDragged;
    public event Action? HandleDragEnd;
    public event Action<IHandleSource, Handle>? HandleSelected;
    public event Action<ISheetElementAction>? ActionInvoked;

    private readonly IToolContext _context;
    private readonly Sheet _sheet;

    private List<IHandleMapEntry> _queryResults;
    private DragState<IHandleMapEntry> _dragState;

    private bool _redrawPending;
    private long _lastMouseMoveEvent;
    private Brush _moveBrush;
    private Brush _adjustBrush;
    private Pen _selectedPen;
    
    public EditHandleSetToolOverlay(IToolContext context,
                                    Sheet sheet,
                                    IEnumerable<ISheetElementAction?> editActions)
    {
        _context = context;
        _sheet = sheet;
        _queryResults = new(128);
        _dragState = new();
        
        _context.Viewport.ViewportChanged += ForceRedraw;
        
        _context.HandleMap.SheetSelectionChanged += ForceRedraw;
        _context.HandleMap.HandleAdded += OnHandleAdded;
        _context.HandleMap.HandleRemoved += OnHandleRemoved;
        _context.HandleMap.HandleMoved += OnHandleMoved;
        _context.HandleMap.HandleSelectionChanged += ForceRedraw;

        _moveBrush = new SolidColorBrush(Color.FromArgb(128, 255, 128, 0));
        _moveBrush.Freeze();

        _adjustBrush = new SolidColorBrush(Color.FromArgb(128, 0, 128, 0));
        _adjustBrush.Freeze();

        _selectedPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)), 2.0);
        _selectedPen.Freeze();
        
        ContextMenu = new ContextMenu();
        ContextMenuOpening += (s, e) => RebuildContextMenu(s, e, editActions);
        
        _context.EditOverlayRenderer.IsEnabled = true;

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
        _context.EditOverlayRenderer.IsEnabled = false;
        _context.Viewport.ViewportChanged -= ForceRedraw;

        _context.HandleMap.SheetSelectionChanged -= ForceRedraw;
        _context.HandleMap.HandleAdded -= OnHandleAdded;
        _context.HandleMap.HandleRemoved -= OnHandleRemoved;
        _context.HandleMap.HandleMoved -= OnHandleMoved;
        _context.HandleMap.HandleSelectionChanged += ForceRedraw;
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
        var mousePosition = e.GetPosition(VisualTreeHelper.GetParent(this) as UIElement);

        var clickPosition = _context.Viewport.FromPoint(mousePosition);
        var clickSizeUnit = _context.Viewport.FromPixels(16);
        var clickSize = new Unit2D(clickSizeUnit, clickSizeUnit);

        var handle = _context.HandleMap.GetClosestHandle(UnitBounds.FromCenterSize(clickPosition, clickSize));

        if (handle is null || !handle.Editing)
        {
            return;
        }
        
        _dragState.OnDragStart(mousePosition,
                               handle,
                               handle.Position);
        
        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (_dragState.IsDragging)
        {
            HandleDragEnd?.Invoke();
        }
        else if (_dragState.DraggedElement is not null)
        {
            HandleSelected?.Invoke(_dragState.DraggedElement.Source,
                                   _dragState.DraggedElement.Handle);
        }

        _dragState.OnDragEnd();
        
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
        
        var dragResult = _dragState.OnDragMove(_context.Viewport,
                                               _context.UnitSnap,
                                               this,
                                               mousePosition,
                                               _dragState.DraggedElement.Position);

        if (dragResult is null)
        {
            return;
        }

        if (dragResult.Value.IsDragBeginning)
        {
            HandleDragBegin?.Invoke(_dragState.DraggedElement.Source,
                                    _dragState.DraggedElement.Handle);
        }

        HandleDragged?.Invoke(_dragState.DraggedElement.Source,
                              _dragState.DraggedElement.Handle,
                              dragResult.Value.ElementPositionDelta);

        e.Handled = true;
    }

    private void OnHandleAdded(IHandleSource source, Handle handle, Unit2D position)
    {
        ForceRedraw();
    }

    private void OnHandleRemoved(IHandleSource source, Handle handle)
    {
        ForceRedraw();
    }

    private void OnHandleMoved(IHandleSource source, Handle handle, Unit2D position)
    {
        ForceRedraw();
    }

    public bool CanUnitSnapTo(IHandleSource source)
    {
        return true;
    }
    
    public bool CanUnitSnapTo(Handle handle)
    {
        if (_context.HandleMap.TryGetHandleEntry(handle, out var entry))
        {
            return !entry.Selected;
        }

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

        _queryResults.Clear();
        _context.HandleMap.QueryHandles(UnitBounds.FromCenterSize(Unit2D.Zero,
                                                                  _context.Viewport.Size),
                                        _queryResults);
        
        foreach (var entry in _queryResults)
        {
            if (!entry.Editing)
            {
                continue;
            }
            
            var point = _context.Viewport.ToPoint(entry.Position);
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
    }
}
