using StencilPad.Spatial;

namespace StencilPad.Models;

public class ImageElement : SheetElement<ImageElement>
{
    public override MinMaxHandleSource HandleSource { get; }

    public Unit2D Min
    {
        get => HandleSource.Min;
        set => HandleSource.Min = value;
    }

    public Unit2D Max
    {
        get => HandleSource.Max;
        set => HandleSource.Max = value;
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
        HandleSource = new MinMaxHandleSource(Unit2D.Zero, Unit2D.Zero);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
    }

    public ImageElement(Unit2D start, Unit2D end, byte[] imageData)
    {
        HandleSource = new MinMaxHandleSource(start, end);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();

        _imageData = imageData;
    }

    public override void MirrorX(Unit centerY)
    {
        Min = new Unit2D(Min.X, (centerY * 2) - Min.Y);
        Max = new Unit2D(Max.X, (centerY * 2) - Max.Y);
    }

    public override void MirrorY(Unit centerX)
    {
        Min = new Unit2D((centerX * 2) - Min.X, Min.Y);
        Max = new Unit2D((centerX * 2) - Max.X, Max.Y);
    }

    public override void Translate(Unit2D delta)
    {
        HandleSource.Min += delta;
        HandleSource.Max += delta;
    }

    public override void AssignFrom(ImageElement other)
    {
        Min = other.Min;
        Max = other.Max;
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
