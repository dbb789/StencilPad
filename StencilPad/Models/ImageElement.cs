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

    public double _opacity = 1.0;
    public double Opacity
    {
        get => _opacity;
        set
        {
            _opacity = value;
            OnPropertyChanged();
        }
    }

    public ImageElement()
    {
        _boundsHandleSource = new BoundsHandleSource(UnitBounds.Empty);
        _boundsHandleSource.HandleMoved += (_, _, _) => FireGeometryChanged();
        SetHandleSource(_boundsHandleSource);
    }

    public ImageElement(Unit2D min, Unit2D max, byte[] imageData)
    {
        _boundsHandleSource = new BoundsHandleSource(UnitBounds.FromMinMax(min, max));
        _boundsHandleSource.HandleMoved += (_, _, _) => FireGeometryChanged();
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

    public override void NormalizePosition()
    {
        var midpoint = _boundsHandleSource.Bounds.Center;
        _boundsHandleSource.Bounds = _boundsHandleSource.Bounds - midpoint;
        Transform = Transform with { Position = Transform.Position + Transform.Rotate(midpoint) };
    }

    public override UnitBounds GetTransformedBounds(UnitTransform transform)
    {
        return _boundsHandleSource.Bounds.ApplyTransform(transform);
    }

    public override void SetTransformedBounds(UnitBounds newBounds, UnitTransform transform)
    {
        _boundsHandleSource.Bounds = UnitBounds.FromMinMax(
            transform.InverseApply(newBounds.Min),
            transform.InverseApply(newBounds.Max));
    }

    public override void AssignFrom(ImageElement other)
    {
        _boundsHandleSource.AssignFrom(other._boundsHandleSource);

        Transform = other.Transform;
        ImageData = other.ImageData;
        Opacity = other.Opacity;
    }

    public override ImageElement DeepClone()
    {
        var clone = new ImageElement();
        
        clone.Id = Id;
        clone.AssignFrom(this);
        
        return clone;
    }
}
