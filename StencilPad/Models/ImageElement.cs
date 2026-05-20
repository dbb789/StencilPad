using StencilPad.Spatial;

namespace StencilPad.Models;

public class ImageElement : SheetElement<ImageElement>
{
    public override BoundsHandleSource HandleSource { get; }

    public Unit2D Min
    {
        get => HandleSource.Bounds.Min;
        set => HandleSource.Bounds = UnitBounds.FromMinMax(value, HandleSource.Bounds.Max);
    }

    public Unit2D Max
    {
        get => HandleSource.Bounds.Max;
        set => HandleSource.Bounds = UnitBounds.FromMinMax(value, HandleSource.Bounds.Max);
    }


    private byte[] _imageData = [];
    public byte[] ImageData
    {
        get => _imageData;
        set
        {
            _imageData = value;
            OnPropertyChanged();
        }
    }

    public event Action? GeometryChanged;
    
    public ImageElement()
    {
        HandleSource = new BoundsHandleSource(UnitBounds.Empty);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
    }

    public ImageElement(Unit2D min, Unit2D max, byte[] imageData)
    {
        HandleSource = new BoundsHandleSource(UnitBounds.FromMinMax(min, max));
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
        _imageData = imageData;
    }

    public override void MirrorX(Unit centerY)
    {
        HandleSource.Bounds = UnitBounds.FromMinMax(
            new Unit2D(Min.X, (centerY * 2) - Min.Y),
            new Unit2D(Max.X, (centerY * 2) - Max.Y)
        );
    }

    public override void MirrorY(Unit centerX)
    {
        HandleSource.Bounds = UnitBounds.FromMinMax(
            new Unit2D((centerX * 2) - Min.X, Min.Y),
            new Unit2D((centerX * 2) - Max.X, Max.Y)
        );
    }

    public override void Translate(Unit2D delta)
    {
        HandleSource.Bounds += delta;
    }

    public override void AssignFrom(ImageElement other)
    {
        HandleSource.AssignFrom(other.HandleSource);
        ImageData = other.ImageData;
    }

    public override ImageElement DeepClone()
    {
        var clone = new ImageElement();
        
        clone.Id = Id;
        clone.AssignFrom(this);
        
        return clone;
    }
}
