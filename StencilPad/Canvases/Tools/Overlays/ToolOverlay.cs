using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Spatial;
using StencilPad.Models;
using StencilPad.Collections;

namespace StencilPad.Canvases.Tools.Overlays;

public abstract class ToolOverlay : Canvas
{
    protected Sheet Sheet => _sheet;
    
    private readonly IViewport _viewport;
    private readonly Sheet _sheet;
    private readonly bool _selectionOnly;
    private readonly List<IToolOverlayRendererFactory> _factories;
    private readonly Dictionary<ISheetElement, IToolOverlayRenderer> _renderers;
    
    public ToolOverlay(IViewport viewport, Sheet sheet, bool selectionOnly)
    {
        _viewport = viewport;
        _sheet = sheet;
        _selectionOnly = selectionOnly;
        _factories = new();
        _renderers = new();

        foreach (var element in GetElements())
        {
            AddRenderer(element);
        }

        GetList().ListChanged += ElementsChanged;        
    }

    public virtual void Dispose()
    {
        GetList().ListChanged -= ElementsChanged;
        
        foreach (var (_, renderer) in _renderers)
        {
            renderer.RendererDirty -= ForceRedraw;
            renderer.Dispose();
        }

        _renderers.Clear();
    }

    private IEnumerable<ISheetElement> GetElements()
    {
        return _selectionOnly ? _sheet.Selection : _sheet.Elements;
    }
    
    private IObservableList<ISheetElement> GetList()
    {
        return _selectionOnly ? _sheet.Selection : _sheet.Elements;
    }

    protected void RegisterOverlay(IToolOverlayRendererFactory factory)
    {
        _factories.Add(factory);

        foreach (var element in GetElements())
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
    
    private void ElementsChanged(ObservableListChangedArgs<ISheetElement> e)
    {
        // NOTE: We're currently ignoring ordering here as it generally shouldn't matter.
        switch (e.Action)
        {
        case ObservableListChangedAction.Add:
            AddRenderer(e.Item);
            break;
            
        case ObservableListChangedAction.Remove:
            RemoveRenderer(e.Item);
            break;
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

        ForceRedraw();
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

        ForceRedraw();
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
