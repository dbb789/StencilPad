using System.ComponentModel;
using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public class ImageElementResolver : IModelResolver
{
    private readonly ImageElement _imageElement;

    private IModelWalker? _walker;
    private IImageWalker? _imageWalker;
    
    public ImageElementResolver(ImageElement imageElement)
    {
        _imageElement = imageElement;
        _imageElement.GeometryChanged += OnGeometryChanged;
        _imageElement.TransformChanged += OnTransformChanged;
        _imageElement.PropertyChanged += OnPropertyChanged;
    }

    public void Dispose()
    {
        Detach();
        
        _imageElement.GeometryChanged -= OnGeometryChanged;
        _imageElement.TransformChanged -= OnTransformChanged;
        _imageElement.PropertyChanged -= OnPropertyChanged;
    }

    public void Attach(IModelWalker walker)
    {
        _walker = walker;
        _walker.SetTransform(_imageElement.Transform);
        
        _imageWalker = walker.CreateImageWalker();
        _imageWalker.SetBounds(UnitBounds.FromMinMax(_imageElement.Min, _imageElement.Max));
        _imageWalker.SetImageData(_imageElement.ImageData);
        _imageWalker.SetOpacity(_imageElement.Opacity);
    }

    public void Detach()
    {
        _imageWalker = null;
        _walker = null;
    }
    
    private void OnGeometryChanged(ISheetElement element)
    {
        _imageWalker?.SetBounds(UnitBounds.FromMinMax(_imageElement.Min, _imageElement.Max));
    }

    private void OnTransformChanged(ISheetElement element)
    {
        _walker?.SetTransform(_imageElement.Transform);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImageElement.ImageData))
        {
            _imageWalker?.SetImageData(_imageElement.ImageData);
        }
        else if (e.PropertyName == nameof(ImageElement.Opacity))
        {
            _imageWalker?.SetOpacity(_imageElement.Opacity);
        }
    }
}
