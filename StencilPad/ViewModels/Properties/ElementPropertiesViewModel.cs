using System.Collections.Specialized;
using StencilPad.Models;

namespace StencilPad.ViewModels.Properties;

public abstract class ElementPropertiesViewModel<TElement> : ViewModelBase, IDisposable
    where TElement : ISheetElement
{
    private bool _hasElements;
    public bool HasElements
    {
        get => _hasElements;
        private set
        {
            _hasElements = value;
            OnPropertyChanged();
        }
    }
    
    protected IEnumerable<TElement> Elements => _elements;
    
    private readonly Sheet _sheet;
    private readonly List<TElement> _elements;

    protected ElementPropertiesViewModel(Sheet sheet)
    {
        _sheet = sheet;
        _elements = _sheet.Selection.OfType<TElement>().ToList();

        HasElements = _elements.Count > 0;

        _sheet.Selection.CollectionChanged += SelectionChanged;
    }

    public void Dispose()
    {
        _sheet.Selection.CollectionChanged -= SelectionChanged;
    }

    private void SelectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is TElement element)
                {
                    _elements.Remove(element);
                }
            }
        }

        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is TElement element)
                {
                    _elements.Add(element);
                }
            }
        }

        HasElements = _elements.Count > 0;
        OnElementsChanged();
    }

    protected virtual void OnElementsChanged()
    {
        // ...
    }

    protected T? Mode<T>(Func<TElement, T> selector) where T : notnull
    {
        var map = new Dictionary<T, int>();

        foreach (var element in _elements)
        {
            var value = selector(element);

            if (map.TryGetValue(value, out var count))
            {
                map[value] = count + 1;
            }
            else
            {
                map[value] = 1;
            }
        }

        T? highest = default;
        int highestCount = 0;

        foreach (var (value, count) in map)
        {
            if (count > highestCount)
            {
                highest = value;
                highestCount = count;
            }
        }

        return highest;
    }
}
