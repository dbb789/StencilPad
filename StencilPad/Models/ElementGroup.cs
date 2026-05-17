using StencilPad.Spatial;

namespace StencilPad.Models;

public class ElementGroup : SheetElement<ElementGroup>
{
    public IEnumerable<ISheetElement> Children => _children;
    public override GroupHandleSource HandleSource { get; }

    private List<ISheetElement> _children;

    public Unit2D _position = Unit2D.Zero;
    public Unit2D Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                HandleSource.Position = value;                
                OnPropertyChanged();
            }
        }
    }

    public event Action? ChildrenChanged;
    
    public ElementGroup()
    {
        _children = new();
        HandleSource = new();
    }
    
    public ElementGroup(IEnumerable<ISheetElement> children)
    {
        _children = new(children.Select(c => c.DeepClone()));
        HandleSource = new(_children.Select(child => child.HandleSource));
    }

    public override void MirrorX(Unit centerY)
    {
        foreach (var child in _children)
        {
            child.MirrorX(centerY);
        }
    }

    public override void MirrorY(Unit centerX)
    {
        foreach (var child in _children)
        {
            child.MirrorY(centerX);
        }
    }
    
    public override void Translate(Unit2D delta)
    {
        Position += delta;
    }

    public override void AssignFrom(ElementGroup other)
    {
        _children = new(other.Children.Select(child => child.DeepClone()));
        HandleSource.SetChildren(_children.Select(child => child.HandleSource));

        ChildrenChanged?.Invoke();
    }

    public override ISheetElement DeepClone()
    {
        var clone = new ElementGroup();

        clone.Id = Id;
        clone.AssignFrom(this);

        return clone;
    }
}
