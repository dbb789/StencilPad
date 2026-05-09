using System.Windows;
using System.Windows.Controls;

namespace StencilPad.Canvases.Tools.Widgets;

public class WidgetContainer<T> where T : UIElement, new()
{
    private readonly Canvas _parentCanvas;
    private readonly List<T> _elements;

    public T this[int index] => _elements[index];
    public int Count => _elements.Count;

    public event Action<T>? WidgetAdded;
    public event Action<T>? WidgetRemoved;

    public WidgetContainer(Canvas parentCanvas)
    {
        _parentCanvas = parentCanvas;
        _elements = [];
    }

    public void Resize(int count)
    {
        while (_elements.Count < count)
        {
            var element = new T();

            _elements.Add(element);
            _parentCanvas.Children.Add(element);

            WidgetAdded?.Invoke(element);
        }

        while (_elements.Count > count)
        {
            var element = _elements[^1];

            _elements.RemoveAt(_elements.Count - 1);
            _parentCanvas.Children.Remove(element);

            WidgetRemoved?.Invoke(element);
        }
    }

    public void Clear()
    {
        Resize(0);
    }

    public List<T>.Enumerator GetEnumerator()
    {
        return _elements.GetEnumerator();
    }
}
