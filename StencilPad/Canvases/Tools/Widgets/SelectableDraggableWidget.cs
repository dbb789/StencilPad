using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace StencilPad.Canvases.Tools.Widgets;

public abstract class SelectableDraggableWidget : FrameworkElement
{
    public bool Draggable;
    public bool Selectable;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                InvalidateVisual();
            }
        }
    }

    public bool IsDragging => _isDragging;

    private bool _isSelected;
    private Point? _dragStart;
    private bool _isDragging;

    public SelectableDraggableWidget()
    {
        Draggable = false;
        Selectable = false;
        IsSelected = false;

        _dragStart = null;
        _isDragging = false;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!Draggable && !Selectable)
        {
            return;
        }

        var mousePosition = e.GetPosition(VisualTreeHelper.GetParent(this) as UIElement);

        _dragStart = mousePosition;

        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!Draggable && !Selectable)
        {
            return;
        }

        if (_dragStart is not null && !_isDragging && Selectable)
        {
            InvokeOnChangeSelection(!IsSelected);
        }

        if (_isDragging)
        {
            _isDragging = false;
            OnDragEnd();
        }
        
        _dragStart = null;

        ReleaseMouseCapture();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!Draggable && !Selectable)
        {
            return;
        }

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
                OnDragBegin();
            }
        }

        if (_isDragging)
        {
            if (Selectable && !IsSelected)
            {
                InvokeOnChangeSelection(true);
            }

            OnDrag(_dragStart.Value, mousePosition);
        }

        e.Handled = true;
    }

    private void InvokeOnChangeSelection(bool selected)
    {
        OnChangeSelection(selected);
    }
    
    protected virtual void OnChangeSelection(bool selected) { }
    protected virtual void OnDragBegin() { }
    protected virtual void OnDrag(Point begin, Point end) { }
    protected virtual void OnDragEnd() { }
}
