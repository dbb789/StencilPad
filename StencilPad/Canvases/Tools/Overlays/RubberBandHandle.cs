using System.Windows;

namespace StencilPad.Canvases.Tools.Overlays;

public class RubberBandHandle
{
    public bool IsDragging => _isDragging;
    public Rect DragBounds => _dragStart is null ?
        Rect.Empty : new Rect(_dragStart.Value, _dragCurrent);
    
    private Point? _dragStart;
    private Point _dragCurrent;
    private bool _isDragging;

    public void DragBegin(Point mousePosition)
    {
        _dragStart = mousePosition;
        _dragCurrent = _dragStart.Value;
        _isDragging = false;
    }

    public bool DragUpdate(Point mousePosition)
    {
        if (_dragStart is null)
        {
            return false;
        }

        _dragCurrent = mousePosition;

        if (!_isDragging)
        {
            var delta = _dragCurrent - _dragStart.Value;

            if (Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDragging = true;
            }
        }

        return true;
    }

    public void DragEnd()
    {
        _dragStart = null;
        _isDragging = false;
    }
}
