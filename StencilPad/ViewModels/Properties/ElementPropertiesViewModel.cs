using System.Collections.Specialized;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.ViewModels.Properties;

public abstract class ElementPropertiesViewModel<TElement> : ViewModelBase, IDisposable
    where TElement : class, ISheetElement, new()
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
    
    public UnitSettings UnitSettings => _settings.UnitSettings;
    protected Sheet Sheet => _sheet;
    protected IOperationService OperationService => _operationService;
    protected IEnumerable<TElement> Elements => _elements;

    private readonly Sheet _sheet;
    private readonly IOperationService _operationService;
    private readonly ISettings _settings;
    private readonly TElement _defaults;
    private readonly List<TElement> _elements;

    protected ElementPropertiesViewModel(Sheet sheet,
                                         IOperationService operationService,
                                         ISettings settings)
    {
        _sheet = sheet;
        _operationService = operationService;
        _settings = settings;
        _defaults = new();
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

    protected void SetElementProperty(Action<TElement> setter)
    {
        using var context = _operationService.TryCreateEditContext(_sheet, _elements);
        
        foreach (var element in _elements)
        {
            setter?.Invoke(element);
        }

        _settings.GetElementStyle(_defaults);
        setter?.Invoke(_defaults);
        _settings.SetElementStyle(_defaults);
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

    protected Unit? All(Func<TElement, Unit> selector)
    {
        if (_elements.Count == 0)
        {
            return null;
        }

        var first = selector(_elements[0]);

        for (int i = 1; i < _elements.Count; ++i)
        {
            if (!first.ApproximatelyEquals(selector(_elements[i])))
            {
                return null;
            }
        }

        return first;
    }
}
