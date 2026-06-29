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
                OnTransformChanged();
            }
        }
    }
    
    public event Action<ISheetElement>? TransformChanged;
    public event Action<ISheetElement>? GeometryChanged;

    private IHandleSource? _elementHandleSource;

    public event Action<ISheetElement, Handle, Unit2D, bool>? HandleAdded;
    public event Action<ISheetElement, Handle>? HandleRemoved;
    public event Action<ISheetElement, Handle, Unit2D>? HandleMoved;
    public event Action<ISheetElement, Handle, bool>? HandleSelectionChanged;

    public void QueryHandles(Action<Handle, Unit2D, bool> func)
    {
        _elementHandleSource?.QueryHandles((handle, position, selected) =>
        {
            func(handle, Transform.Apply(position), selected);
        });
    }

    public void SetHandleSelected(Handle handle, bool selected)
    {
        _elementHandleSource?.SetHandleSelected(handle, selected);
    }

    public Unit2D GetPoint(Handle handle)
    {
        return Transform.Apply(_elementHandleSource?.GetPoint(handle) ?? Unit2D.Zero);
    }
    
    public void SetPoint(Handle handle, Unit2D position)
    {
        _elementHandleSource?.SetPoint(handle, Transform.InverseApply(position));
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

    public UnitBounds GetBounds()
    {
        return GetTransformedBounds(Transform);
    }

    public virtual bool ContainsPoint(Unit2D point)
    {
        var localPoint = Transform.InverseApply(point);
        var bounds = GetTransformedBounds(UnitTransform.Identity);

        var size = bounds.Size;

        size = new Unit2D(Unit.Max(size.X, Unit.FromMillimeters(1)),
                          Unit.Max(size.Y, Unit.FromMillimeters(1)));

        bounds = UnitBounds.FromCenterSize(bounds.Center, size);
        
        return bounds.Contains(localPoint);
    }

    public virtual bool IntersectsBounds(UnitBounds bounds)
    {
        var elementBounds = GetBounds();
        
        return elementBounds.Intersects(bounds);
    }
    
    private void InvokeHandleAdded(IHandleSource source, Handle handle, Unit2D position, bool selected)
    {
        HandleAdded?.Invoke(this, handle, Transform.Apply(position), selected);
    }

    private void InvokeHandleRemoved(IHandleSource source, Handle handle)
    {
        HandleRemoved?.Invoke(this, handle);
    }

    private void InvokeHandleMoved(IHandleSource source, Handle handle, Unit2D position)
    {
        HandleMoved?.Invoke(this, handle, Transform.Apply(position));
    }

    private void InvokeHandleSelectionChanged(IHandleSource source, Handle handle, bool selected)
    {
        HandleSelectionChanged?.Invoke(this, handle, selected);
    }

    private void MoveAllHandles()
    {
        if (_elementHandleSource is null)
        {
            return;
        }

        _elementHandleSource.QueryHandles(MoveAllInvoke);
    }

    private void MoveAllInvoke(Handle handle, Unit2D position, bool selected)
    {
        if (_elementHandleSource is null)
        {
            return;
        }
        
        InvokeHandleMoved(_elementHandleSource, handle, position);
    }
    
    protected void FireGeometryChanged()
    {
        GeometryChanged?.Invoke(this);
    }

    public abstract void MirrorX(Unit centerY);
    public abstract void MirrorY(Unit centerX);
    public abstract void NormalizePosition();
    public abstract UnitBounds GetTransformedBounds(UnitTransform transform);
    public abstract void SetBounds(UnitBounds newBounds, UnitTransform transform);
    public abstract void AssignFromElement(ISheetElement other);
    public abstract ISheetElement DeepClone();

    protected virtual void OnTransformChanged()
    {
        TransformChanged?.Invoke(this);
        MoveAllHandles();
    }
}
