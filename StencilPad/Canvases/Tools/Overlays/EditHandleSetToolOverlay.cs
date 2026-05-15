using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Rendering;
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
    public event Action<ISheetElement, Handle, Unit2D>? HandleDragged;
    public event Action? HandleDragEnd;
    
    public event Action<ISheetElement, Handle, bool>? HandleSelectionChanged;
    public event Action<ISheetElementAction>? ActionInvoked;

    private readonly IToolContext _context;
    private readonly Sheet _sheet;
    private readonly List<ISheetElement> _selection;
    private readonly EditOverlayRenderer _editOverlayRenderer;

    public EditHandleSetToolOverlay(IToolContext context,
                                    Sheet sheet,
                                    IEnumerable<ISheetElementAction?> editActions)
    {
        _context = context;
        _sheet = sheet;
        _selection = [];
        _editOverlayRenderer = context.EditOverlayRenderer;

        _context.HandleMap.HandleAdded += HandleAdded;
        _context.HandleMap.HandleRemoved += HandleRemoved;
        _context.HandleMap.HandleMoved += HandleMoved;
        
        _editOverlayRenderer.InvalidateVisual += InvalidateVisual;
        _context.Viewport.ViewportChanged += InvalidateVisual;

        Rebuild();

        ContextMenu = new ContextMenu();
        ContextMenuOpening += (s, e) => RebuildContextMenu(s, e, editActions);
    }

    public void Dispose()
    {
        _context.HandleMap.HandleAdded -= HandleAdded;
        _context.HandleMap.HandleRemoved -= HandleRemoved;
        _context.HandleMap.HandleMoved -= HandleMoved;

        _editOverlayRenderer.InvalidateVisual -= InvalidateVisual;
        _context.Viewport.ViewportChanged -= InvalidateVisual;
    }

    private void RebuildContextMenu(object sender,
                                    ContextMenuEventArgs e,
                                    IEnumerable<ISheetElementAction?> actions)
    {
        var subSelection = _selection.Where(e => e.HandleSource.GetSelectedHandles().Any());

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
    
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        dc.PushTransform(_context.Viewport.GetMillimetersToPixelsTransform());

        _editOverlayRenderer.Render(dc);

        dc.Pop();
        
        var handleList = new List<(HandleMapEntry, Unit2D)>();

        var pageSize = new Unit2D(Unit.FromMillimeters(1000), Unit.FromMillimeters(1000));

        _context.HandleMap.QueryHandles(UnitBounds.FromCenterSize(Unit2D.Zero, pageSize), handleList);
        
        var moveBrush = new SolidColorBrush(Color.FromArgb(128, 255, 128, 0));
        var adjustBrush = new SolidColorBrush(Color.FromArgb(128, 0, 128, 0));
        var selectedPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)), 1);

        moveBrush.Freeze();
        adjustBrush.Freeze();
        selectedPen.Freeze();
        
        foreach (var (entry, position) in handleList)
        {
            var point = _context.Viewport.ToPoint(position);

            bool selected = entry.Source.GetSelectedHandles().Contains(entry.Handle);
            var pen = selected ? selectedPen : null;
           
            if (entry.Handle.Type == HandleType.Move)
            {
                dc.DrawRectangle(moveBrush, pen, new Rect(point.X - 6, point.Y - 6, 12, 12));
            }
            else
            {
                dc.DrawEllipse(adjustBrush, pen, point, 6, 6);
            }
        }
    }

    private void HandleAdded(IHandleSource handleSet, Handle handle, Unit2D position)
    {
        Rebuild();
    }

    private void HandleRemoved(IHandleSource handleSet, Handle handle)
    {
        Rebuild();
    }
    
    private void HandleMoved(IHandleSource handleSet, Handle handle, Unit2D position)
    {
        Rebuild();
    }

    private void Rebuild()
    {
        // var entries = _selection.SelectMany(e => e.HandleSource.Handles.Select(
        //                                         h => new HandleEntry(Element: e, Handle: h)))
        //     .ToList();

        // _widgetContainer.Resize(entries.Count);
        // _widgetMap.Clear();

        // for (int i = 0; i < entries.Count; ++i)
        // {
        //     var (element, handle) = entries[i];
        //     var widget = _widgetContainer[i];

        //     widget.Handle = handle;
        //     _widgetMap.Add(handle, new WidgetEntry(element, widget));
        // }

        // RepositionAll();
        // UpdateSelection();
    }

    private void OnWidgetDragBegin(HandleWidget widget)
    {
        // HandleDragBegin?.Invoke();
    }
    
    private void OnWidgetDragged(HandleWidget widget, Point start, Point position)
    {
        // var handle = widget.Handle;
        
        // if (!_widgetMap.TryGetValue(handle, out var entry))
        // {
        //     return;
        // }
        
        // var newPosition = _context.UnitSnap.UnitSnap(_context.Viewport.FromPoint(position));
        // var delta = newPosition - entry.Element.HandleSource.GetPoint(handle);

        // if (delta == Unit2D.Zero)
        // {
        //     return;
        // }
        
        // HandleDragged?.Invoke(entry.Element,
        //                       handle,
        //                       delta);
    }

    private void OnWidgetDragEnd(HandleWidget widget)
    {
        // HandleDragEnd?.Invoke();
    }
}
