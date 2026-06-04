using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Spatial;
using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Overlays;

public abstract class ToolOverlay : Canvas
{
    private readonly IViewport _viewport;
    private readonly Sheet _sheet;
    private readonly List<IToolOverlayRendererFactory> _factories;
    private readonly Dictionary<ISheetElement, IToolOverlayRenderer> _renderers;
    
    public ToolOverlay(IViewport viewport, Sheet sheet)
    {
        _viewport = viewport;
        _sheet = sheet;
        _factories = new();
        _renderers = new();

        foreach (var element in _sheet.Selection)
        {
            AddRenderer(element);
        }

        _sheet.Selection.CollectionChanged += ElementsChanged;        
    }

    public virtual void Dispose()
    {
        _sheet.Selection.CollectionChanged -= ElementsChanged;
        
        foreach (var (_, renderer) in _renderers)
        {
            renderer.RendererDirty -= ForceRedraw;
            renderer.Dispose();
        }

        _renderers.Clear();
    }

    protected void RegisterOverlay(IToolOverlayRendererFactory factory)
    {
        _factories.Add(factory);

        foreach (var element in _sheet.Selection)
        {
            if (!_renderers.ContainsKey(element))
            {
                var renderer = factory.CreateOverlay(element);

                if (renderer is not null)
                {
                    renderer.RendererDirty += ForceRedraw;
                    _renderers.Add(element, renderer);
                }
            }
        }

        foreach (var renderer in _renderers.Values)
        {
            if (renderer is GroupToolOverlayRenderer groupRenderer)
            {
                groupRenderer.RegisterOverlay(factory);
            }
        }
    }

    protected void RenderOverlay(DrawingContext dc)
    {
        dc.PushTransform(_viewport.MillimetersToPixelsTransform);
        
        foreach (var (_, renderer) in _renderers)
        {
            renderer.Render(dc);
        }

        dc.Pop();
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
                    AddRenderer(element);
                }
            }
        }
    }
    
    private void AddRenderer(ISheetElement element)
    {
        IToolOverlayRenderer? renderer;
        
        if (element is ElementGroup group)
        {
            // Special case for groups.
            renderer = new GroupToolOverlayRenderer(group, _factories);
        }
        else
        {
            renderer = CreateRenderer(element);
        }
        
        if (renderer is null)
        {
            return;
        }

        renderer.RendererDirty += ForceRedraw;

        _renderers.Add(element, renderer);
    }

    private void RemoveRenderer(ISheetElement element)
    {
        if (!_renderers.TryGetValue(element, out var renderer))
        {
            return;
        }
        
        renderer.RendererDirty -= ForceRedraw;
        renderer.Dispose();
        _renderers.Remove(element);
    }
    
    private IToolOverlayRenderer? CreateRenderer(ISheetElement element)
    {
        foreach (var factory in _factories)
        {
            var renderer = factory.CreateOverlay(element);

            if (renderer is not null)
            {
                return renderer;
            }
        }

        return null;
    }

    protected void ForceRedraw()
    {
        InvalidateVisual();
    }
}
