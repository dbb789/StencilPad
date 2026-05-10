using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Rendering;
using StencilPad.Canvases.Tools.Widgets;
using StencilPad.Canvases.Tools.Controllers.Actions;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class EditHandleSetToolOverlay : Canvas, IDisposable
{
    private record struct HandleEntry(ISheetElement Element, Handle Handle);

    public IEnumerable<ISheetElement> Selection
    {
        get => _selection;
        set
        {
            foreach (var element in _selection)
            {
                var handleSet = element.HandleSet;

                handleSet.HandlesChanged -= Rebuild;
                handleSet.SelectionChanged -= UpdateSelection;
            }

            _selection.Clear();
            _selection.AddRange(value);

            foreach (var element in _selection)
            {
                var handleSet = element.HandleSet;

                handleSet.HandlesChanged += Rebuild;
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

    private readonly Sheet _sheet;
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly List<ISheetElement> _selection;
    private readonly EditOverlayRenderer _editOverlayRenderer;

    private readonly WidgetContainer<HandleWidget> _widgets;
    private List<HandleEntry> _handleMap = [];

    public EditHandleSetToolOverlay(Sheet sheet,
                                    IViewport viewport,
                                    IUnitSnap unitSnap,
                                    SheetElementEditActions sheetElementEditActions,
                                    EditOverlayRenderer editOverlayRenderer)
    {
        _sheet = sheet;
        _viewport = viewport;
        _unitSnap = unitSnap;
        _selection = [];
        _editOverlayRenderer = editOverlayRenderer;

        _widgets = new WidgetContainer<HandleWidget>(this);
        _widgets.WidgetAdded += OnWidgetAdded;

        foreach (var element in _selection)
        {
            var handleSet = element.HandleSet;
            
            handleSet.HandlesChanged += Rebuild;
            handleSet.SelectionChanged += UpdateSelection;
        }

        _editOverlayRenderer.InvalidateVisual += InvalidateVisual;
        _viewport.ViewportChanged += Reposition;

        Rebuild();

        ContextMenu = new ContextMenu();
        ContextMenuOpening += (_, _) => RebuildContextMenu(sheetElementEditActions);
    }

    public void Dispose()
    {
        foreach (var element in _selection)
        {
            var handleSet = element.HandleSet;
            
            handleSet.HandlesChanged -= Rebuild;
            handleSet.SelectionChanged -= UpdateSelection;
        }

        _editOverlayRenderer.InvalidateVisual -= InvalidateVisual;
        _viewport.ViewportChanged -= Reposition;
        _widgets.WidgetAdded -= OnWidgetAdded;
    }

    private void RebuildContextMenu(SheetElementEditActions sheetElementEditActions)
    {
        ContextMenu.Items.Clear();

        var actions = sheetElementEditActions.Create(_selection);

        foreach (var action in actions)
        {
            if (action.IsVisible(_sheet, _selection))
            {
                var menuItem = new MenuItem
                {
                    Header = action.Name,
                };

                menuItem.IsEnabled = action.IsEnabled(_sheet, _selection);
                menuItem.Click += (s, e) =>
                {
                    ActionInvoked?.Invoke(action);
                };

                ContextMenu.Items.Add(menuItem);
            }
        }
    }
    
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        dc.PushTransform(_viewport.GetMillimetersToPixelsTransform());
        _editOverlayRenderer.Render(dc);
        dc.Pop();
    }

    private void OnWidgetAdded(HandleWidget widget)
    {
        widget.Dragged += OnWidgetDragged;
        widget.ChangeSelection += OnWidgetSelectionChanged;
        widget.DragBegin += OnWidgetDragBegin;
        widget.DragEnd += OnWidgetDragEnd;
    }

    private void Rebuild()
    {
        _handleMap = _selection
            .SelectMany(s => s.HandleSet.Handles.Select(h => new HandleEntry(s, h)))
            .ToList();

        _widgets.Resize(_handleMap.Count);

        for (int i = 0; i < _handleMap.Count; ++i)
        {
            _widgets[i].Handle = _handleMap[i].Handle;
        }
        
        Reposition();
        UpdateSelection();
    }

    private void Reposition()
    {
        for (int i = 0; i < _handleMap.Count; ++i)
        {
            var entry = _handleMap[i];
            var point = _viewport.ToPoint(entry.Element.HandleSet.GetPoint(entry.Handle));
            
            SetLeft(_widgets[i], point.X);
            SetTop(_widgets[i], point.Y);
        }
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < _handleMap.Count; ++i)
        {
            var entry = _handleMap[i];
            
            _widgets[i].IsSelected = entry.Element.HandleSet.GetSelectedHandles().Contains(entry.Handle);
        }
    }

    private void OnWidgetDragBegin(HandleWidget widget)
    {
        HandleDragBegin?.Invoke();
    }
    
    private void OnWidgetDragged(HandleWidget widget, Point start, Point position)
    {
        var index = GetWidgetIndex(widget);

        if (index < 0)
        {
            return;
        }
        
        var entry = _handleMap[index];
        var newPosition = _unitSnap.UnitSnap(_viewport.FromPoint(position));
        var delta = newPosition - entry.Element.HandleSet.GetPoint(entry.Handle);

        if (delta == Unit2D.Zero)
        {
            return;
        }
        
        HandleDragged?.Invoke(entry.Element,
                              entry.Handle,
                              delta);
    }

    private void OnWidgetDragEnd(HandleWidget widget)
    {
        HandleDragEnd?.Invoke();
    }

    private void OnWidgetSelectionChanged(HandleWidget widget, bool selected)
    {
        var index = GetWidgetIndex(widget);

        if (index < 0)
        {
            return;
        }
        
        var entry = _handleMap[index];
        
        HandleSelectionChanged?.Invoke(entry.Element,
                                       entry.Handle,
                                       selected);
    }

    private int GetWidgetIndex(HandleWidget widget)
    {
        for (int i = 0; i < _widgets.Count; ++i)
        {
            if (_widgets[i] == widget)
            {
                return i;
            }
        }
        
        return -1;
    }
}
