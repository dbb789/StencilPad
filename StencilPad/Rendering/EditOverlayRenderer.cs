using System.Collections.Specialized;
using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Rendering;

public class EditOverlayRenderer : IEditOverlayRenderer
{
    public Sheet? Sheet
    {
        get => _sheet;
        set => AssignSheet(value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetEnabled(value);
    }

    private Sheet? _sheet;
    private bool _isEnabled;
    private readonly Dictionary<ISheetElement, SheetElementEditRenderer> _renderers;

    public event Action? RendererDirty;

    public EditOverlayRenderer()
    {
        _isEnabled = false;
        _renderers = new();
    }

    public void Render(DrawingContext dc)
    {
        if (!_isEnabled)
        {
            return;
        }
        
        foreach (var renderer in _renderers.Values)
        {
            renderer.Render(dc);
        }
    }

    public void Dispose()
    {
        IsEnabled = false;
        AssignSheet(null);
    }

    private void AssignSheet(Sheet? sheet)
    {
        if (_sheet == sheet)
        {
            return;
        }

        if (_isEnabled)
        {
            Unsubscribe();
        }
        
        _sheet = sheet;

        if (_isEnabled)
        {
            Subscribe();
        }
    }

    private void SetEnabled(bool enabled)
    {
        if (_isEnabled == enabled)
        {
            return;
        }

        _isEnabled = enabled;

        if (_isEnabled)
        {
            Subscribe();
        }
        else
        {
            Unsubscribe();
        }
        
        RendererDirty?.Invoke();
    }

    private void Subscribe()
    {
        if (_sheet is null)
        {
            return;
        }
        
        _sheet.Selection.CollectionChanged += SelectionChanged;
        
        RebuildRenderers();
    }

    private void Unsubscribe()
    {
        if (_sheet is null)
        {
            return;
        }
        
        _sheet.Selection.CollectionChanged -= SelectionChanged;
        
        ClearRenderers();
    }

    private void SelectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
                    AddRenderer(element);
                }
            }
        }

        InvokeRendererDirty();
    }

    private void RebuildRenderers()
    {
        ClearRenderers();

        if (_sheet is null)
        {
            return;
        }
        
        foreach (var element in _sheet.Selection)
        {
            AddRenderer(element);
        }
    }

    private void ClearRenderers()
    {
        foreach (var element in _renderers.Keys.ToList())
        {
            RemoveRenderer(element);
        }
        
        _renderers.Clear();
    }
    
    private void AddRenderer(ISheetElement element)
    {
        var renderer = SheetElementEditRendererFactory.Create(element);

        if (renderer is not null)
        {
            renderer.RendererDirty += InvokeRendererDirty;
            _renderers[element] = renderer;
        }
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
