using StencilPad.Spatial;

namespace StencilPad.Models;

public class ImageElement : SheetElement<ImageElement>
{
    private BoundsHandleSource _boundsHandleSource;

    public Unit2D Min
    {
        get => _boundsHandleSource.Bounds.Min;
        set => _boundsHandleSource.Bounds = UnitBounds.FromMinMax(value, _boundsHandleSource.Bounds.Max);
    }

    public Unit2D Max
    {
        get => _boundsHandleSource.Bounds.Max;
        set => _boundsHandleSource.Bounds = UnitBounds.FromMinMax(_boundsHandleSource.Bounds.Min, value);
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
        _boundsHandleSource = new BoundsHandleSource(UnitBounds.Empty);
        _boundsHandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
        SetHandleSource(_boundsHandleSource);
    }

    public ImageElement(Unit2D min, Unit2D max, byte[] imageData)
    {
        _boundsHandleSource = new BoundsHandleSource(UnitBounds.FromMinMax(min, max));
        _boundsHandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
        SetHandleSource(_boundsHandleSource);
        _imageData = imageData;
    }

    public override void MirrorX(Unit centerY)
    {
        Transform = Transform with 
        { 
            Position = Transform.Position with { Y = (centerY * 2) - Transform.Position.Y },
            Angle = -Transform.Angle
        };
    }

    public override void MirrorY(Unit centerX)
    {
        Transform = Transform with 
        { 
            Position = Transform.Position with { X = (centerX * 2) - Transform.Position.X },
            Angle = -Transform.Angle
        };
    }

    public override void Translate(Unit2D delta)
    {
        Transform = Transform with { Position = Transform.Position + delta };
    }

    public override void AssignFrom(ImageElement other)
    {
        _boundsHandleSource.AssignFrom(other._boundsHandleSource);
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
