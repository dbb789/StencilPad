using StencilPad.Models;
using StencilPad.Models.Operations;

namespace StencilPad.Controllers;

public class UndoStack
{
    private const int Capacity = 100;
    
    private List<IOperation> _stack;
    private int _index;

    public UndoStack()
    {
        _stack = new(16);
        _index = -1;
    }
    
    public void Push(IOperation operation)
    {
        // Clear redo operations - this is a simple list and not a tree.
        for (var i = _stack.Count - 1; i > _index; --i)
        {
            _stack.RemoveAt(i);
        }
        
        _stack.Add(operation);
        ++_index;

        while (_stack.Count > Capacity)
        {
            _stack.RemoveAt(0);
            --_index;
        }
    }

    public void Undo(Project project, out Sheet? targetSheet)
    {
        targetSheet = null;
        
        if (_index < 0)
        {
            return;
        }

        _stack[_index].Invert().Execute(project, out targetSheet);
        
        --_index;
    }

    public void Redo(Project project, out Sheet? targetSheet)
    {
        targetSheet = null;
        
        if (_index >= _stack.Count - 1)
        {
            return;
        }

        ++_index;
        
        _stack[_index].Execute(project, out targetSheet);
    }

    public void Clear()
    {
        _stack.Clear();
        _index = -1;
    }
}
