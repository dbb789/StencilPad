using System.Collections.Specialized;
using System.Windows.Media;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;

namespace StencilPad.Rendering;

public class SheetRenderer : IDisposable
{
    public Sheet? Sheet
    {
        get => _sheet;
        set => AssignSheet(value);
    }

    private readonly ISettings _settings;
    private readonly IResourceSet _resourceSet;
    private Sheet? _sheet;
    private OrderedDictionary<ISheetElement, SheetElementRenderer> _renderers;
    
    public event Action? RendererDirty;

    public SheetRenderer(ISettings settings,
                         IResourceSet resourceSet)
    {
        _settings = settings;
        _resourceSet = resourceSet;
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
            renderer.RendererDirty -= InvokeRendererDirty;
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

        InvokeRendererDirty();
    }

    private void ElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is ISheetElement element)
                {
                    RemoveRenderer(element);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is ISheetElement element)
                {
                    AddRenderer(element, e.NewStartingIndex);
                }
            }
        }

        InvokeRendererDirty();
    }
    
    private void AddRenderer(ISheetElement element, int index = -1)
    {
        var renderer = new SheetElementRenderer(element, _settings, _resourceSet);

        if (!renderer.HasContent)
        {
            renderer.Dispose();
            return;
        }

        renderer.RendererDirty += InvokeRendererDirty;

        if (index < 0)
        {
            index = _renderers.Count;
        }

        _renderers.Insert(index, element, renderer);
    }

    private void RemoveRenderer(ISheetElement element)
    {
        if (_renderers.TryGetValue(element, out var renderer))
        {
            renderer.RendererDirty -= InvokeRendererDirty;
            renderer.Dispose();
            _renderers.Remove(element);
        }
    }
    
    private void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
