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
                TransformChanged?.Invoke(this);
            }
        }
    }

    public event Action<ISheetElement>? TransformChanged;
    public event Action<ISheetElement>? GeometryChanged;

    protected void FireGeometryChanged() => GeometryChanged?.Invoke(this);

    private IHandleSource? _elementHandleSource;

    public event Action<ISheetElement, Handle, Unit2D, bool>? HandleAdded;
    public event Action<ISheetElement, Handle>? HandleRemoved;
    public event Action<ISheetElement, Handle, Unit2D>? HandleMoved;
    public event Action<ISheetElement, Handle, bool>? HandleSelectionChanged;

    public void QueryHandles(Action<Handle, Unit2D, bool> func)
    {
        _elementHandleSource?.QueryHandles(func);
    }

    public void SetHandleSelected(Handle handle, bool selected)
    {
        _elementHandleSource?.SetHandleSelected(handle, selected);
    }

    public Unit2D GetPoint(Handle handle)
    {
        return _elementHandleSource?.GetPoint(handle) ?? Unit2D.Zero;
    }
    
    public void SetPoint(Handle handle, Unit2D position)
    {
        _elementHandleSource?.SetPoint(handle, position);
    }

    protected void SetHandleSource(IHandleSource newHandleSource)
    {
        if (_elementHandleSource is not null)
        {
            _elementHandleSource.HandleAdded -= InvokeHandleAdded;
            _elementHandleSource.HandleRemoved -= InvokeHandleRemoved;
            _elementHandleSource.HandleMoved -= InvokeHandleMoved;
            _elementHandleSource.HandleSelectionChanged -= InvokeHandleSelectionChanged;
        }

        _elementHandleSource = newHandleSource;

        if (_elementHandleSource is not null)
        {
            _elementHandleSource.HandleAdded += InvokeHandleAdded;
            _elementHandleSource.HandleRemoved += InvokeHandleRemoved;
            _elementHandleSource.HandleMoved += InvokeHandleMoved;
            _elementHandleSource.HandleSelectionChanged += InvokeHandleSelectionChanged;
        }
    }

    public UnitBounds GetTransformedBounds()
    {
        return GetBounds(Transform);
    }
    
    private void InvokeHandleAdded(IHandleSource source, Handle handle, Unit2D position, bool selected)
    {
        HandleAdded?.Invoke(this, handle, position, selected);
    }

    private void InvokeHandleRemoved(IHandleSource source, Handle handle)
    {
        HandleRemoved?.Invoke(this, handle);
    }

    private void InvokeHandleMoved(IHandleSource source, Handle handle, Unit2D position)
    {
        HandleMoved?.Invoke(this, handle, position);
    }

    private void InvokeHandleSelectionChanged(IHandleSource source, Handle handle, bool selected)
    {
        HandleSelectionChanged?.Invoke(this, handle, selected);
    }
    
    public abstract void MirrorX(Unit centerY);
    public abstract void MirrorY(Unit centerX);
    public abstract void Translate(Unit2D delta);
    public abstract void NormalizePosition();
    public abstract UnitBounds GetBounds(UnitTransform transform);
    public abstract void SetBounds(UnitBounds newBounds, UnitTransform transform);
    public abstract void AssignFromElement(ISheetElement other);
    public abstract ISheetElement DeepClone();
}
