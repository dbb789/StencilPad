using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Rendering;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class SelectionToolOverlay : FrameworkElement, IDisposable
{
    private SheetRenderer _sheetRenderer;
    private Sheet _sheet;
    private IViewport _viewport;
    private IUnitSnap _unitSnap;

    private Point? _dragPixelStart;
    private Unit2D _dragUnitStart;
    private UnitBounds _dragSelectionBounds;
    private bool _draggingSelection;
    
    public event Action<Unit2D>? SelectionDragged;
    public event Action<Unit2D>? PointSelected;
    public event Action? Group;
    public event Action? Ungroup;
    public event Action? MirrorX;
    public event Action? MirrorY;
    public event Action? ShowProperties;
    public event Action? JustifyTop;
    public event Action? JustifyMiddle;
    public event Action? JustifyBottom;
    public event Action? JustifyLeft;
    public event Action? JusifyCentre;
    public event Action? JustifyRight;
    
    public SelectionToolOverlay(SheetRenderer sheetRenderer,
                                Sheet sheet,
                                IViewport viewport,
                                IUnitSnap unitSnap)
    {
        _sheetRenderer = sheetRenderer;
        _sheet = sheet;
        _viewport = viewport;
        _unitSnap = unitSnap;
        _sheet.Selection.CollectionChanged += SelectionChanged;

        InitializeContextMenu();
    }

    public void Dispose()
    {
        _sheet.Selection.CollectionChanged -= SelectionChanged;
    }

    private void InitializeContextMenu()
    {
        var contextMenu = new ContextMenu();
        
        var groupItem = new MenuItem { Header = "Group", IsEnabled = true };
        groupItem.Click += (s, e) => Group?.Invoke();
        contextMenu.Items.Add(groupItem);
        
        var ungroupItem = new MenuItem { Header = "Ungroup", IsEnabled = true };
        ungroupItem.Click += (s, e) => Ungroup?.Invoke();
        contextMenu.Items.Add(ungroupItem);

        contextMenu.Items.Add(new Separator());

        var mirrorXMenuItem = new MenuItem { Header = "Flip Horizontally", IsEnabled = false };
        mirrorXMenuItem.Click += (s, e) => MirrorX?.Invoke();
        contextMenu.Items.Add(mirrorXMenuItem);

        var mirrorYMenuItem = new MenuItem { Header = "Flip Vertically", IsEnabled = false };
        mirrorYMenuItem.Click += (s, e) => MirrorY?.Invoke();
        contextMenu.Items.Add(mirrorYMenuItem);

        contextMenu.Items.Add(new Separator());

        var propertiesMenuItem = new MenuItem { Header = "Properties…", IsEnabled = false };
        propertiesMenuItem.Click += (s, e) => ShowProperties?.Invoke();
        contextMenu.Items.Add(propertiesMenuItem);

        contextMenu.Items.Add(new Separator());

        var justifyTopMenuItem    = new MenuItem { Header = "Justify Top",    IsEnabled = false };
        var justifyMiddleMenuItem = new MenuItem { Header = "Justify Middle", IsEnabled = false };
        var justifyBottomMenuItem = new MenuItem { Header = "Justify Bottom", IsEnabled = false };
        justifyTopMenuItem.Click    += (s, e) => JustifyTop?.Invoke();
        justifyMiddleMenuItem.Click += (s, e) => JustifyMiddle?.Invoke();
        justifyBottomMenuItem.Click += (s, e) => JustifyBottom?.Invoke();
        contextMenu.Items.Add(justifyTopMenuItem);
        contextMenu.Items.Add(justifyMiddleMenuItem);
        contextMenu.Items.Add(justifyBottomMenuItem);

        contextMenu.Items.Add(new Separator());

        var justifyLeftMenuItem   = new MenuItem { Header = "Justify Left",   IsEnabled = false };
        var justifyCentreMenuItem = new MenuItem { Header = "Justify Centre", IsEnabled = false };
        var justifyRightMenuItem  = new MenuItem { Header = "Justify Right",  IsEnabled = false };
        justifyLeftMenuItem.Click   += (s, e) => JustifyLeft?.Invoke();
        justifyCentreMenuItem.Click += (s, e) => JusifyCentre?.Invoke();
        justifyRightMenuItem.Click  += (s, e) => JustifyRight?.Invoke();
        contextMenu.Items.Add(justifyLeftMenuItem);
        contextMenu.Items.Add(justifyCentreMenuItem);
        contextMenu.Items.Add(justifyRightMenuItem);

        contextMenu.Opened += (s, e) =>
        {
            var hasPolygons = _sheet.Selection.OfType<IPolygonSheetElement>().Any();
            var multipleSelected = _sheet.Selection.Count >= 2;
            mirrorXMenuItem.IsEnabled = hasPolygons;
            mirrorYMenuItem.IsEnabled = hasPolygons;
            propertiesMenuItem.IsEnabled = _sheet.Selection.OfType<MarkerPath>().Any();
            justifyTopMenuItem.IsEnabled    = multipleSelected;
            justifyMiddleMenuItem.IsEnabled = multipleSelected;
            justifyBottomMenuItem.IsEnabled = multipleSelected;
            justifyLeftMenuItem.IsEnabled   = multipleSelected;
            justifyCentreMenuItem.IsEnabled = multipleSelected;
            justifyRightMenuItem.IsEnabled  = multipleSelected;
        };

        ContextMenu = contextMenu;
    }


    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        var mousePosition = e.GetPosition(this);

        if (PointIsOverSelection(_viewport.FromPoint(mousePosition)))
        {
            var bounds = GetSelectionBounds();
            if (bounds.HasValue)
            {
                _dragPixelStart = mousePosition;
                _dragUnitStart = _viewport.FromPoint(mousePosition);
                _dragSelectionBounds = bounds.Value;
                e.Handled = true;
            }
            return;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_dragPixelStart.HasValue)
        {
            var mousePosition = e.GetPosition(this);
            var pixelDelta = mousePosition - _dragPixelStart.Value;

            if (_draggingSelection ||
                Math.Abs(pixelDelta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(pixelDelta.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _draggingSelection = true;

                var currentUnit = _viewport.FromPoint(mousePosition);
                var displacement = currentUnit - _dragUnitStart;

                var currentBounds = GetSelectionBounds();
                if (currentBounds.HasValue)
                {
                    var delta = SnapDelta(_dragSelectionBounds, displacement, currentBounds.Value);
                    SelectionDragged?.Invoke(delta);
                }

                e.Handled = true;
            }
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!_draggingSelection)
        {
            var mousePosition = _viewport.FromPoint(e.GetPosition(this));
            
            PointSelected?.Invoke(mousePosition);
            e.Handled = true;
        }

        if (_dragPixelStart.HasValue)
        {
            _dragPixelStart = null;
            e.Handled = true;
        }
        
        _draggingSelection = false;
    }
    
    private Unit2D SnapDelta(UnitBounds dragBounds, Unit2D displacement, UnitBounds currentBounds)
    {
        // The 4 corners of the selection as they would be after applying the raw displacement.
        // Each desired corner has a corresponding current corner (same index).
        Span<Unit2D> desiredCorners =
        [
            dragBounds.Min + displacement,
            new Unit2D(dragBounds.Max.X, dragBounds.Min.Y) + displacement,
            new Unit2D(dragBounds.Min.X, dragBounds.Max.Y) + displacement,
            dragBounds.Max + displacement,
        ];

        Span<Unit2D> currentCorners =
        [
            currentBounds.Min,
            new Unit2D(currentBounds.Max.X, currentBounds.Min.Y),
            new Unit2D(currentBounds.Min.X, currentBounds.Max.Y),
            currentBounds.Max,
        ];

        int bestIndex = 0;
        double bestError = double.MaxValue;

        for (int i = 0; i < desiredCorners.Length; i++)
        {
            var snapped = _unitSnap.UnitSnap(desiredCorners[i]);
            var error = (snapped - desiredCorners[i]).Magnitude.Millimeters;

            if (error < bestError)
            {
                bestError = error;
                bestIndex = i;
            }
        }

        return _unitSnap.UnitSnap(desiredCorners[bestIndex]) - currentCorners[bestIndex];
    }

    private bool PointIsOverSelection(Unit2D point)
    {
        foreach (var selected in _sheet.Selection)
        {
            if (_sheetRenderer.TryGetElementRenderer(selected, out var renderer) &&
                renderer.HitTest(point))
            {
                return true;
            }
        }

        return false;
    }
    
    private UnitBounds? GetSelectionBounds()
    {
        UnitBounds? selectionBounds = null;

        foreach (var selected in _sheet.Selection)
        {
            if (_sheetRenderer.TryGetElementRenderer(selected, out var renderer))
            {
                if (selectionBounds.HasValue)
                {
                    selectionBounds = UnitBounds.Union(selectionBounds.Value, renderer.SelectionBounds);
                }
                else
                {
                    selectionBounds = renderer.SelectionBounds;
                }
            }
        }

        return selectionBounds;
    }

    private void SelectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is SheetElement element &&
                    _sheetRenderer.TryGetElementRenderer(element, out var renderer))
                {
                    renderer.InvalidateVisual -= InvalidateVisual;
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is SheetElement element &&
                    _sheetRenderer.TryGetElementRenderer(element, out var renderer))
                {
                    renderer.InvalidateVisual += InvalidateVisual;
                }
            }
        }
        
        InvalidateVisual();
    }
    
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        dc.PushTransform(_viewport.GetMillimetersToPixelsTransform());

        var pen = new Pen(Brushes.Blue, 0.2);
        var fill = new SolidColorBrush(Color.FromArgb(10, 0, 0, 255));

        foreach (var selected in _sheet.Selection)
        {
            if (_sheetRenderer.TryGetElementRenderer(selected, out var renderer))
            {
                var bounds = renderer.SelectionBounds;

                dc.DrawRectangle(fill, pen, bounds.Millimeters);
            }
        }

        dc.Pop();
    }
}
