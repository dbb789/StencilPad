using System.Collections.Specialized;
using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Canvases.Rendering;

public class SheetRenderer : IDisposable
{
    public class Factory(ISheetElementRendererFactory SheetElementRendererFactory)
    {
        public SheetRenderer Create()
        {
            return new SheetRenderer(SheetElementRendererFactory);
        }
    }
    
    public Sheet? Sheet
    {
        get => _sheet;
        set => AssignSheet(value);
    }

    public SheetElementRenderer this[int index] => _renderers.GetAt(index).Value;
    public int Count => _renderers.Count;

    private readonly ISheetElementRendererFactory _sheetElementRendererFactory;
    private Sheet? _sheet;
    private OrderedDictionary<ISheetElement, SheetElementRenderer> _renderers;

    public event Action? InvalidateVisual;

    private SheetRenderer(ISheetElementRendererFactory sheetElementRendererFactory)
    {
        _sheetElementRendererFactory = sheetElementRendererFactory;
        _renderers = new();
    }

    public void Dispose()
    {
        AssignSheet(null);
    }

    public void Render(DrawingContext dc)
    {
        if (_sheet is null)
        {
            return;
        }

        foreach (var (_, renderer) in _renderers)
        {
            renderer.Render(dc);
        }
    }
    
    public bool TryGetElementRenderer(ISheetElement element, out SheetElementRenderer renderer)
    {
        return _renderers.TryGetValue(element, out renderer!);
    }

    private void AssignSheet(Sheet? sheet)
    {
        if (_sheet == sheet)
        {
            return;
        }
        
        if (_sheet is not null)
        {
            _sheet.Elements.CollectionChanged -= ElementsChanged;
        }

        _sheet = sheet;

        if (_sheet is not null)
        {
            _sheet.Elements.CollectionChanged += ElementsChanged;
        }

        foreach (var renderer in _renderers.Values)
        {
            renderer.InvalidateVisual -= InvokeInvalidateVisual;
            renderer.Dispose();
        }
        
        _renderers.Clear();
        
        if (_sheet is not null)
        {
            foreach (var element in _sheet.Elements)
            {
                AddRenderer(element);
            }
        }

        InvokeInvalidateVisual();
    }

    private void ElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is SheetElement element)
                {
                    RemoveRenderer(element);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is SheetElement element)
                {
                    AddRenderer(element, e.NewStartingIndex);
                }
            }
        }

        InvokeInvalidateVisual();
    }
    
    private void AddRenderer(ISheetElement element, int index = -1)
    {
        var renderer = _sheetElementRendererFactory.Create(element);

        if (renderer is not null)
        {
            renderer.InvalidateVisual += InvokeInvalidateVisual;

            if (index < 0)
            {
                index = _renderers.Count;
            }
            
            _renderers.Insert(index, element, renderer);
        }
    }

    private void RemoveRenderer(ISheetElement element)
    {
        if (_renderers.TryGetValue(element, out var renderer))
        {
            renderer.InvalidateVisual -= InvokeInvalidateVisual;
            renderer.Dispose();
            _renderers.Remove(element);
        }
    }
    
    private void InvokeInvalidateVisual()
    {
        InvalidateVisual?.Invoke();
    }
}
