using StencilPad.Spatial;

namespace StencilPad.Models;

public class ImageElement : SheetElement<ImageElement>
{
    public override BoundsHandleSource HandleSource { get; }

    public override UnitTransform Transform
    {
        get => HandleSource.Transform;
        set
        {
            if (HandleSource.Transform != value)
            {
                HandleSource.Transform = value;
                OnPropertyChanged();
                GeometryChanged?.Invoke();
            }
        }
    }

    public Unit2D Min
    {
        get => HandleSource.Bounds.Min;
        set => HandleSource.Bounds = UnitBounds.FromMinMax(value, HandleSource.Bounds.Max);
    }

    public Unit2D Max
    {
        get => HandleSource.Bounds.Max;
        set => HandleSource.Bounds = UnitBounds.FromMinMax(HandleSource.Bounds.Min, value);
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
