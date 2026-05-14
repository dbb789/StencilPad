using StencilPad.Spatial;

namespace StencilPad.Models;

public class ImageElement : SheetElement<ImageElement>
{
    public override StartEndHandleSource HandleSource { get; }

    public Unit2D Start
    {
        get => HandleSource.Start;
        set => HandleSource.Start = value;
    }

    public Unit2D End
    {
        get => HandleSource.End;
        set => HandleSource.End = value;
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
        HandleSource = new StartEndHandleSource(Unit2D.Zero, Unit2D.Zero);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
    }

    public ImageElement(Unit2D start, Unit2D end, byte[] imageData)
    {
        HandleSource = new StartEndHandleSource(start, end);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();

        _imageData = imageData;
    }

    public override void MirrorX(Unit centerY)
    {
        Start = new Unit2D(Start.X, (centerY * 2) - Start.Y);
        End = new Unit2D(End.X, (centerY * 2) - End.Y);
    }

    public override void MirrorY(Unit centerX)
    {
        Start = new Unit2D((centerX * 2) - Start.X, Start.Y);
        End = new Unit2D((centerX * 2) - End.X, End.Y);
    }

    public override void Translate(Unit2D delta)
    {
        HandleSource.Start += delta;
        HandleSource.End += delta;
    }

    public override void AssignFrom(ImageElement other)
    {
        Start = other.Start;
        End = other.End;
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
