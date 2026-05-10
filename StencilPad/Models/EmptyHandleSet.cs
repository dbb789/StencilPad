using StencilPad.Spatial;

namespace StencilPad.Models;

public class EmptyHandleSet : IHandleSet
{
    public static readonly EmptyHandleSet Instance = new();
    
    public event Action? HandlesChanged
    {
        add { }
        remove { }
    }
    
    public event Action? SelectionChanged
    {
        add { }
        remove { }
    }

    public IEnumerable<Handle> Handles =>  Enumerable.Empty<Handle>();
    
    public Unit2D GetPoint(Handle handle)
    {
        throw new InvalidOperationException("EmptyHandleSet does not contain any handles.");
    }

    public void SetPoint(Handle handle, Unit2D position)
    {
        throw new InvalidOperationException("EmptyHandleSet does not contain any handles.");
    }

    public IEnumerable<Handle> GetSelectedHandles()
    {
        return Enumerable.Empty<Handle>();
    }

    public void SetSelectedHandles(IEnumerable<Handle> handles)
    {
        if (handles.Any())
        {
            throw new InvalidOperationException("EmptyHandleSet cannot have any selected handles.");
        }
    }

    public EmptyHandleSet DeepClone()
    {
        return this;
    }
}
