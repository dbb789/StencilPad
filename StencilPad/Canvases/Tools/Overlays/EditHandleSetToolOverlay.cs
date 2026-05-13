using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
    
    public IEnumerable<ISheetElement> Selection
    {
        get => _selection;
        set
        {
            foreach (var element in _selection)
            {
                var handleSet = element.HandleSet;

                handleSet.HandlesChanged -= Rebuild;
                handleSet.HandleMoved -= Reposition;
                handleSet.SelectionChanged -= UpdateSelection;
            }

            _selection.Clear();
            _selection.AddRange(value);

            foreach (var element in _selection)
            {
                var handleSet = element.HandleSet;

                handleSet.HandlesChanged += Rebuild;
                handleSet.HandleMoved += Reposition;
                handleSet.SelectionChanged += UpdateSelection;
            }

            Rebuild();
        }
    }

    public event Action? HandleDragBegin;
    public event Action<ISheetElement, Handle, Unit2D>? HandleDragged;
    public event Action? HandleDragEnd;
    
    public event Action<ISheetElement, Handle, bool>? HandleSelectionChanged;
    public event Action<ISheetElementAction>? ActionInvoked;

    private readonly IToolContext _context;
    private readonly Sheet _sheet;
    private readonly List<ISheetElement> _selection;
    private readonly EditOverlayRenderer _editOverlayRenderer;

    private Dictionary<Handle, WidgetEntry> _widgetMap = [];
    private readonly WidgetContainer<HandleWidget> _widgetContainer;

    public EditHandleSetToolOverlay(IToolContext context,
                                    Sheet sheet,
                                    IEnumerable<ISheetElementAction?> editActions)
    {
        _context = context;
        _sheet = sheet;
        _selection = [];
        _editOverlayRenderer = context.EditOverlayRenderer;

        _widgetMap = new();
        _widgetContainer = new(this);
        _widgetContainer.WidgetAdded += WidgetAdded;

        foreach (var element in _selection)
        {
            var handleSet = element.HandleSet;
            
            handleSet.HandlesChanged += Rebuild;
            handleSet.HandleMoved += Reposition;
            handleSet.SelectionChanged += UpdateSelection;
        }

        _editOverlayRenderer.InvalidateVisual += InvalidateVisual;
        _context.Viewport.ViewportChanged += RepositionAll;

        Rebuild();

        ContextMenu = new ContextMenu();
        ContextMenuOpening += (s, e) => RebuildContextMenu(s, e, editActions);
    }

    public void Dispose()
    {
        foreach (var element in _selection)
        {
            var handleSet = element.HandleSet;
            
            handleSet.HandlesChanged -= Rebuild;
            handleSet.HandleMoved -= Reposition;
            handleSet.SelectionChanged -= UpdateSelection;
        }

        _widgetContainer.WidgetAdded -= WidgetAdded;

        _editOverlayRenderer.InvalidateVisual -= InvalidateVisual;
        _context.Viewport.ViewportChanged -= RepositionAll;
    }

    private void RebuildContextMenu(object sender,
                                    ContextMenuEventArgs e,
                                    IEnumerable<ISheetElementAction?> actions)
    {
        var subSelection = _selection.Where(e => e.HandleSet.GetSelectedHandles().Any());

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
    }

    private void WidgetAdded(HandleWidget widget)
    {
        widget.Dragged += OnWidgetDragged;
        widget.ChangeSelection += OnWidgetSelectionChanged;
        widget.DragBegin += OnWidgetDragBegin;
        widget.DragEnd += OnWidgetDragEnd;
    }

    private void Rebuild()
    {
        var entries = _selection.SelectMany(e => e.HandleSet.Handles.Select(
                                                h => new HandleEntry(Element: e, Handle: h)))
            .ToList();

        _widgetContainer.Resize(entries.Count);
        _widgetMap.Clear();

        for (int i = 0; i < entries.Count; ++i)
        {
            var (element, handle) = entries[i];
            var widget = _widgetContainer[i];

            widget.Handle = handle;
            _widgetMap.Add(handle, new WidgetEntry(element, widget));
        }

        RepositionAll();
        UpdateSelection();
    }

    private void Reposition(Handle handle, Unit2D position)
    {
        if (_widgetMap.TryGetValue(handle, out var entry))
        {
            var point = _context.Viewport.ToPoint(position);

            SetLeft(entry.Widget, point.X);
            SetTop(entry.Widget, point.Y);
        }
    }
    
    private void RepositionAll()
    {
        foreach (var (handle, entry) in _widgetMap)
        {
            var point = _context.Viewport.ToPoint(entry.Element.HandleSet.GetPoint(handle));

            SetLeft(entry.Widget, point.X);
            SetTop(entry.Widget, point.Y);
        }
    }

    private void UpdateSelection()
    {
        foreach (var element in _selection)
        {
            var handleSet = element.HandleSet;

            foreach (var handle in handleSet.Handles)
            {
                if (_widgetMap.TryGetValue(handle, out var entry))
                {
                    entry.Widget.IsSelected = handleSet.GetSelectedHandles().Contains(handle);
                }
            }
        }
    }

    private void OnWidgetDragBegin(HandleWidget widget)
    {
        HandleDragBegin?.Invoke();
    }
    
    private void OnWidgetDragged(HandleWidget widget, Point start, Point position)
    {
        var handle = widget.Handle;
        
        if (!_widgetMap.TryGetValue(handle, out var entry))
        {
            return;
        }
        
        var newPosition = _context.UnitSnap.UnitSnap(_context.Viewport.FromPoint(position));
        var delta = newPosition - entry.Element.HandleSet.GetPoint(handle);

        if (delta == Unit2D.Zero)
        {
            return;
        }
        
        HandleDragged?.Invoke(entry.Element,
                              handle,
                              delta);
    }

    private void OnWidgetDragEnd(HandleWidget widget)
    {
        HandleDragEnd?.Invoke();
    }

    private void OnWidgetSelectionChanged(HandleWidget widget, bool selected)
    {
        var handle = widget.Handle;
        
        if (!_widgetMap.TryGetValue(handle, out var entry))
        {
            return;
        }

        HandleSelectionChanged?.Invoke(entry.Element,
                                       handle,
                                       selected);
    }
}
