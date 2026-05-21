using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class ImageElementRenderer : SheetElementRenderer
{
    public override ImageElement Element => _imageElement;

    public override UnitBounds SelectionBounds =>
        _imageElement.Min == _imageElement.Max
            ? UnitBounds.Empty
            : UnitBounds.FromMinMax(_imageElement.Min, _imageElement.Max).ApplyTransform(_imageElement.Transform);

    private readonly ImageElement _imageElement;
    private BitmapImage? _bitmap;

    public ImageElementRenderer(ImageElement imageElement)
    {
        _imageElement = imageElement;
        _imageElement.GeometryChanged += InvokeRendererDirty;
        _imageElement.TransformChanged += InvokeRendererDirty;
        _imageElement.PropertyChanged += OnPropertyChanged;
        
        RebuildBitmap();
    }

    public override void Dispose()
    {
        _imageElement.GeometryChanged -= InvokeRendererDirty;
        _imageElement.TransformChanged -= InvokeRendererDirty;
        _imageElement.PropertyChanged -= OnPropertyChanged;
    }

    public override bool HitTest(Unit2D unit)
    {
        var localUnit = _imageElement.Transform.InverseApply(unit);
        return UnitBounds.FromMinMax(_imageElement.Min, _imageElement.Max).Contains(localUnit);
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        // Transform the selection bounds into the local space of the image.
        var localNW = _imageElement.Transform.InverseApply(bounds.NW);
        var localNE = _imageElement.Transform.InverseApply(bounds.NE);
        var localSW = _imageElement.Transform.InverseApply(bounds.SW);
        var localSE = _imageElement.Transform.InverseApply(bounds.SE);

        var localSelectionBounds = UnitBounds.FromMinMax(
            new Unit2D(Unit.Min(Unit.Min(localNW.X, localNE.X), Unit.Min(localSW.X, localSE.X)),
                       Unit.Min(Unit.Min(localNW.Y, localNE.Y), Unit.Min(localSW.Y, localSE.Y))),
            new Unit2D(Unit.Max(Unit.Max(localNW.X, localNE.X), Unit.Max(localSW.X, localSE.X)),
                       Unit.Max(Unit.Max(localNW.Y, localNE.Y), Unit.Max(localSW.Y, localSE.Y))));

        return localSelectionBounds.Intersects(UnitBounds.FromMinMax(_imageElement.Min, _imageElement.Max));
    }
    
    public override void Render(DrawingContext dc)
    {
        if (_bitmap is null || _imageElement.ImageData.Length == 0)
        {
            return;
        }

        var rect = UnitBounds.FromMinMax(_imageElement.Min, _imageElement.Max).Millimeters;

        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        dc.PushTransform(_imageElement.Transform.CreateGroupTransform());
        dc.DrawImage(_bitmap, rect);
        dc.Pop();
    }

    private void RebuildBitmap()
    {
        if (_imageElement.ImageData.Length == 0)
        {
            _bitmap = null;
            return;
        }

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = new MemoryStream(_imageElement.ImageData);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        _bitmap = bitmap;
    }

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImageElement.ImageData))
        {
            RebuildBitmap();
        }

        InvokeRendererDirty();
    }

    /// <summary>
    /// Decodes the natural size of the image in millimetres, using the image's own DPI.
    /// Caps the larger dimension at <paramref name="maxMm"/> to keep placement sensible.
    /// </summary>
    public static Unit2D MeasureNaturalSize(byte[] imageData, double maxMm = 150.0)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = new MemoryStream(imageData);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();

        var dpiX = bitmap.DpiX > 0 ? bitmap.DpiX : 96.0;
        var dpiY = bitmap.DpiY > 0 ? bitmap.DpiY : 96.0;

        var widthMm  = bitmap.PixelWidth  * 25.4 / dpiX;
        var heightMm = bitmap.PixelHeight * 25.4 / dpiY;

        var larger = Math.Max(widthMm, heightMm);
        if (larger > maxMm)
        {
            var scale = maxMm / larger;
            widthMm  *= scale;
            heightMm *= scale;
        }

        return new Unit2D(Unit.FromMillimeters(widthMm), Unit.FromMillimeters(heightMm));
    }
}
