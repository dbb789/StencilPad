using System.Windows;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public class DragState<T>
{
    public readonly struct DragResult
    {
        public T DraggedElement { get; }
        public Unit2D TargetElementPosition { get; }
        public bool IsDragBeginning { get; }

        public DragResult(T draggedElement,
                          Unit2D targetElementPosition,
                          bool isDragBeginning)
        {
            DraggedElement = draggedElement;
            TargetElementPosition = targetElementPosition;
            IsDragBeginning = isDragBeginning;
        }
    }

    public Unit2D InitialElementPosition => _initialElementPosition ?? Unit2D.Zero;
    public bool DragStarted => _initialMousePosition.HasValue;
    public bool IsDragging => _isDragging;
    public T DraggedElement => _draggedElement!;
    
    private Point? _initialMousePosition;
    private Unit2D? _initialElementPosition;
    private T? _draggedElement;
    private bool _isDragging;
    
    public DragState()
    {
        _initialMousePosition = null;
        _initialElementPosition = null;
        _draggedElement = default;
        _isDragging = false;
    }
    
    public void OnDragStart(Point mousePosition,
                            T draggedElement,
                            Unit2D elementPosition)
    {
        _initialMousePosition = mousePosition;
        _draggedElement = draggedElement;
        _initialElementPosition = elementPosition;
    }

    public void OnDragEnd()
    {
        _initialMousePosition = null;
        _initialElementPosition = null;
        _draggedElement = default;
        _isDragging = false;
    }

    public DragResult? OnDragMove(IViewport viewport,
                                  Point mousePosition)
    {
        if (_initialMousePosition is null ||
            _initialElementPosition is null ||
            _draggedElement == null)
        {
            return null;
        }

        bool isDragBeginning = false;
        var dragDelta = mousePosition - _initialMousePosition.Value;

        if (!_isDragging)
        {
            if (Math.Abs(dragDelta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(dragDelta.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDragging = true;
                isDragBeginning = true;
            }
        }

        if (_isDragging)
        {
            var elementTargetPosition = _initialElementPosition.Value + viewport.FromPixels(dragDelta.X, dragDelta.Y);

            return new DragResult(_draggedElement,
                                  elementTargetPosition,
                                  isDragBeginning);
        }

        return null;
    }
}
