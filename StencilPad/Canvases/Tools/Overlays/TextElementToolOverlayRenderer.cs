using System.ComponentModel;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class TextElementToolOverlayRenderer : IToolOverlayRenderer
{
    public static readonly IToolOverlayRendererFactory Factory = new FactoryImpl();
    
    private class FactoryImpl : IToolOverlayRendererFactory
    {
        public IToolOverlayRenderer? CreateOverlay(ISheetElement element)
        {
            if (element is TextElement textElement)
            {
                return new TextElementToolOverlayRenderer(textElement);
            }

            return null;
        }
    }
    
    private static Pen OutlinePen;

    static TextElementToolOverlayRenderer()
    {
        OutlinePen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), 0.2)
        {
            DashStyle = DashStyles.Dot
        };
        
        OutlinePen.Freeze();
    }

    private readonly TextElement _textElement;

    public event Action? RendererDirty;

    private TextElementToolOverlayRenderer(TextElement textElement)
    {
        _textElement = textElement;
        _textElement.GeometryChanged += GeometryChanged;
        _textElement.TransformChanged += OnTransformChanged;
        _textElement.PropertyChanged += OnPropertyChanged;
    }

    public void Dispose()
    {
        _textElement.GeometryChanged -= GeometryChanged;
        _textElement.TransformChanged -= OnTransformChanged;
        _textElement.PropertyChanged -= OnPropertyChanged;
    }

    public void Render(DrawingContext dc)
    {
        var bounds = UnitBounds.FromMinMax(_textElement.Min, _textElement.Max);
        
        if (bounds.Size == Unit2D.Zero)
        {
            return;
        }

        var transform = _textElement.Transform.CreateGroupTransform();
        
        dc.PushTransform(transform);
        dc.DrawRectangle(Brushes.Transparent, OutlinePen, bounds.Millimeters);
        dc.Pop();
    }

    private void OnTransformChanged(ISheetElement _)
    {
        InvokeRendererDirty();
    }

    private void GeometryChanged(ISheetElement _)
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
