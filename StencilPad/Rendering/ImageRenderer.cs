using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StencilPad.Models.Resolvers;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class ImageRenderer : IImageWalker, IWalkerRenderer
{
    private static readonly Transform FlipY;

    static ImageRenderer()
    {
        FlipY = new ScaleTransform(1, -1);
        FlipY.Freeze();
    }
    
    private UnitBounds? _bounds;
    private BitmapImage? _bitmap;

    public event Action? RendererDirty;
    
    public ImageRenderer()
    {
        _bounds = null;
        _bitmap = null;
    }

    public void Dispose()
    {
        // ...
    }

    public void SetBounds(UnitBounds? bounds)
    {
        _bounds = bounds;
        InvokeRendererDirty();
    }
    
    public void SetImageData(byte[] imageData)
    {
        if (imageData.Length == 0)
        {
            _bitmap = null;
            InvokeRendererDirty();
            return;
        }
        
        _bitmap = new BitmapImage();
        _bitmap.BeginInit();
        _bitmap.StreamSource = new MemoryStream(imageData);
        _bitmap.CacheOption = BitmapCacheOption.OnLoad;
        _bitmap.EndInit();
        _bitmap.Freeze();

        InvokeRendererDirty();
    }
    
    public void Render(DrawingContext dc)
    {
        if (_bitmap is null || _bounds is null)
        {
            return;
        }

        var flippedBounds = UnitBounds.FromCenterSize(new Unit2D(_bounds.Value.Center.X, -_bounds.Value.Center.Y),
                                                      _bounds.Value.Size);

        var rect = flippedBounds.Millimeters;

        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }
        
        // Account for WPF's inverted Y-axis by flipping the Y-axis for image rendering.
        dc.PushTransform(FlipY);
        dc.DrawImage(_bitmap, rect);
        dc.Pop();
    }
    
    private void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
