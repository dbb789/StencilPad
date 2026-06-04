using System.ComponentModel;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class ImageElementToolOverlayRenderer : IToolOverlayRenderer
{
    public static readonly IToolOverlayRendererFactory Factory = new FactoryImpl();
    
    private class FactoryImpl : IToolOverlayRendererFactory
    {
        public IToolOverlayRenderer? CreateOverlay(ISheetElement element)
        {
            if (element is ImageElement imageElement)
            {
                return new ImageElementToolOverlayRenderer(imageElement);
            }

            return null;
        }
    }
    
    private static Pen OutlinePen;

    static ImageElementToolOverlayRenderer()
    {
        OutlinePen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), 0.2)
        {
            DashStyle = DashStyles.Dot
        };
        
        OutlinePen.Freeze();
    }
    
    private readonly ImageElement _imageElement;
    
    public event Action? RendererDirty;

    private ImageElementToolOverlayRenderer(ImageElement imageElement)
    {
        _imageElement = imageElement;
        _imageElement.GeometryChanged += OnGeometryChanged;
        _imageElement.TransformChanged += OnTransformChanged;
        _imageElement.PropertyChanged += OnPropertyChanged;
    }

    public void Dispose()
    {
        _imageElement.GeometryChanged -= OnGeometryChanged;
        _imageElement.TransformChanged -= OnTransformChanged;
        _imageElement.PropertyChanged -= OnPropertyChanged;
    }

    public void Render(DrawingContext dc)
    {
        var bounds = UnitBounds.FromMinMax(_imageElement.Min, _imageElement.Max);
        
        if (bounds.Size == Unit2D.Zero)
        {
            return;
        }

        var transform = _imageElement.Transform.CreateGroupTransform();
        
        dc.PushTransform(transform);
        dc.DrawRectangle(Brushes.Transparent, OutlinePen, bounds.Millimeters);
        dc.Pop();
    }

    private void OnTransformChanged(ISheetElement _)
    {
        InvokeRendererDirty();
    }
    
    private void OnGeometryChanged(ISheetElement _)
    {
        InvokeRendererDirty();
    }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvokeRendererDirty();
    }

    private void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
