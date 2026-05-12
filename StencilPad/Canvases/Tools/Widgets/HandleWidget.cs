using System.Windows;
using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Widgets;

public class HandleWidget : SelectableDraggableWidget
{
    public Handle Handle
    {
        get => _handle;
        set
        {
            _handle = value;
            InvalidateVisual();
        }
    }
    
    private Handle _handle;
        
    public event Action<HandleWidget, bool>? ChangeSelection;
    public event Action<HandleWidget>? DragBegin;
    public event Action<HandleWidget, Point, Point>? Dragged;
    public event Action<HandleWidget>? DragEnd;

    public HandleWidget()
    {
        Width = 12;
        Height = 12;
        Draggable = true;
        Selectable = true;
    }

    protected override void OnChangeSelection(bool selected)
    {
        ChangeSelection?.Invoke(this, selected);
    }

    protected override void OnDragBegin()
    {
        DragBegin?.Invoke(this);
    }
    
    protected override void OnDrag(Point begin, Point end)
    {
        Dragged?.Invoke(this, begin, end);
    }

    protected override void OnDragEnd()
    {
        DragEnd?.Invoke(this);
    }
    
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null,
            new Rect(0, 0, RenderSize.Width, RenderSize.Height));

        if (Handle.Type == HandleType.Move)
        {
            var pen = new Pen(IsSelected ? Brushes.Blue : Brushes.Transparent, 2.0);
            var geometry = new RectangleGeometry(
                new Rect(new Point(-Width / 2, -Height / 2), new Size(Width, Height)));
            
            dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(128, 255, 128, 0)), pen, geometry);
        }
        else
        {
            var pen = new Pen(IsSelected ? Brushes.Blue : Brushes.Transparent, 2.0);
            var geometry = new EllipseGeometry(new Point(0, 0), Width / 2, Height / 2);
            
            dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(128, 0, 128, 0)), pen, geometry);
        }
    }
}
