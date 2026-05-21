using StencilPad.Spatial;

namespace StencilPad.Models;

public abstract class SheetElement<TSelf> : SheetElement where TSelf : SheetElement<TSelf>
{
    public abstract void AssignFrom(TSelf other);

    public override void AssignFromElement(ISheetElement other)
    {
        if (other is not TSelf tOther)
        {
            throw new ArgumentException($"Expected element of type {typeof(TSelf).Name} but got {other.GetType().Name}");
        }

        AssignFrom(tOther);
    }
}

public abstract class SheetElement : ModelBase, ISheetElement
{
    private UnitTransform _transform = UnitTransform.Identity;
    public UnitTransform Transform
    {
        get => _transform;
        set
        {
            if (_transform != value)
            {
                _transform = value;
                TransformChanged?.Invoke();
            }
        }
    }

    public event Action? TransformChanged;

    public abstract IHandleSource HandleSource { get; }

    public event Action<IHandleSource, Handle, Unit2D, bool>? HandleAdded
    {
        add => HandleSource.HandleAdded += value;
        remove => HandleSource.HandleAdded -= value;
    }

    public event Action<IHandleSource, Handle>? HandleRemoved
    {
        add => HandleSource.HandleRemoved += value;
        remove => HandleSource.HandleRemoved -= value;
    }

    public event Action<IHandleSource, Handle, Unit2D>? HandleMoved
    {
        add => HandleSource.HandleMoved += value;
        remove => HandleSource.HandleMoved -= value;
    }

    public event Action<IHandleSource, Handle, bool>? HandleSelectionChanged
    {
        add => HandleSource.HandleSelectionChanged += value;
        remove => HandleSource.HandleSelectionChanged -= value;
    }

    public void QueryHandles(Action<Handle, Unit2D, bool> func) => HandleSource.QueryHandles(func);
    public void SetHandleSelected(Handle handle, bool selected) => HandleSource.SetHandleSelected(handle, selected);
    public Unit2D GetPoint(Handle handle) => HandleSource.GetPoint(handle);
    public void SetPoint(Handle handle, Unit2D position) => HandleSource.SetPoint(handle, position);

    public abstract void MirrorX(Unit centerY);
    public abstract void MirrorY(Unit centerX);
    public abstract void Translate(Unit2D delta);
    public abstract void AssignFromElement(ISheetElement other);
    public abstract ISheetElement DeepClone();
}
