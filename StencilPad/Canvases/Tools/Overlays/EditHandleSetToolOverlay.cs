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

public class EditHandleSetToolOverlay : Canvas, IDisposable
{
    private record struct HandleEntry(ISheetElement Element, Handle Handle);
    private record struct WidgetEntry(ISheetElement Element, HandleWidget Widget);
    
    public event Action? HandleDragBegin;
    public event Action<IHandleSource, Handle, Unit2D>? HandleDragged;
    public event Action? HandleDragEnd;
    public event Action<IHandleSource, Handle>? HandleSelected;
    public event Action<ISheetElementAction>? ActionInvoked;

    private readonly IToolContext _context;
    private readonly Sheet _sheet;

    private List<HandleMapEntry> _queryResults;
    private Brush _moveBrush;
    private Brush _adjustBrush;
    private Pen _selectedPen;
    
    private Point? _dragStart;
    private bool _isDragging;
    private HandleMapEntry? _dragHandle;
    
    public EditHandleSetToolOverlay(IToolContext context,
                                    Sheet sheet,
                                    IEnumerable<ISheetElementAction?> editActions)
    {
        _context = context;
        _sheet = sheet;
        _queryResults = new(128);
        
        _context.Viewport.ViewportChanged += InvalidateVisual;
        
        _context.HandleMap.SheetSelectionChanged += InvalidateVisual;
        _context.HandleMap.HandleAdded += OnHandleAdded;
        _context.HandleMap.HandleRemoved += OnHandleRemoved;
        _context.HandleMap.HandleMoved += OnHandleMoved;
        _context.HandleMap.HandleSelectionChanged += InvalidateVisual;

        _moveBrush = new SolidColorBrush(Color.FromArgb(128, 255, 128, 0));
        _moveBrush.Freeze();

        _adjustBrush = new SolidColorBrush(Color.FromArgb(128, 0, 128, 0));
        _adjustBrush.Freeze();

        _selectedPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)), 1);
        _selectedPen.Freeze();
        
        ContextMenu = new ContextMenu();
        ContextMenuOpening += (s, e) => RebuildContextMenu(s, e, editActions);
        
        _context.EditOverlayRenderer.IsEnabled = true;
    }

    public void Dispose()
    {
        _context.EditOverlayRenderer.IsEnabled = false;
        _context.Viewport.ViewportChanged -= InvalidateVisual;

        _context.HandleMap.SheetSelectionChanged -= InvalidateVisual;
        _context.HandleMap.HandleAdded -= OnHandleAdded;
        _context.HandleMap.HandleRemoved -= OnHandleRemoved;
        _context.HandleMap.HandleMoved -= OnHandleMoved;
        _context.HandleMap.HandleSelectionChanged += InvalidateVisual;
    }

    private void RebuildContextMenu(object sender,
                                    ContextMenuEventArgs e,
                                    IEnumerable<ISheetElementAction?> actions)
    {
        var subSelection = _sheet.Selection.Where(e => e.HandleSource.GetSelectedHandles().Any());

        if (!ContextMenuUtil.RebuildContextMenu(ContextMenu,
                                                _context,
                                                _sheet,
                                                subSelection,
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
        var clickSizeUnit = _context.Viewport.FromPixels(12);
        var clickSize = new Unit2D(clickSizeUnit, clickSizeUnit);
        var queryResults = new List<HandleMapEntry>(4);
        
        _context.HandleMap.QueryHandles(UnitBounds.FromCenterSize(clickPosition, clickSize), queryResults);

        if (queryResults.Count == 0)
        {
            return;
        }
        
        _dragStart = mousePosition;
        _dragHandle = queryResults[0];

        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (_dragStart is null)
        {
            return;
        }

        if (_isDragging)
        {
            _isDragging = false;
            HandleDragEnd?.Invoke();
        }
        else
        {
            if (_dragHandle is not null)
            {
                HandleSelected?.Invoke(_dragHandle.Source, _dragHandle.Handle);
            }
        }

        _dragStart = null;
        _dragHandle = null;
        
        ReleaseMouseCapture();
        e.Handled = true;
    }
    
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_dragStart is null)
        {
            return;
        }

        var mousePosition = e.GetPosition(VisualTreeHelper.GetParent(this) as UIElement);

        if (!_isDragging)
        {
            var delta = mousePosition - _dragStart.Value;

            if (Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDragging = true;
                HandleDragBegin?.Invoke();
            }
        }

        if (_isDragging && _dragHandle is not null)
        {
            var newPosition = _context.UnitSnap.UnitSnap(_context.Viewport.FromPoint(mousePosition), _dragHandle.Handle);
            var delta = newPosition - _dragHandle.Source.GetPoint(_dragHandle.Handle);
            
            HandleDragged?.Invoke(_dragHandle.Source,
                                  _dragHandle.Handle,
                                  delta);
        }

        e.Handled = true;
    }

    private void OnHandleAdded(IHandleSource source, Handle handle, Unit2D position)
    {
        InvalidateVisual();
    }

    private void OnHandleRemoved(IHandleSource source, Handle handle)
    {
        InvalidateVisual();
    }

    private void OnHandleMoved(IHandleSource source, Handle handle, Unit2D position)
    {
        InvalidateVisual();
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
            if (!entry.ElementSelected)
            {
                continue;
            }
            
            var point = _context.Viewport.ToPoint(entry.Position);
            var pen = entry.HandleSelected ? _selectedPen : null;
           
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
