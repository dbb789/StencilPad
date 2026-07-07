using StencilPad.Models;
using StencilPad.Models.Operations;

namespace StencilPad.Controllers;

public class UndoStack
{
    public bool SaveState => _saveState;
    
    private const int Capacity = 100;
    
    private List<IOperation> _stack;
    private int _index;
    private int _saveIndex;
    private bool _saveState;
    
    public event Action? SaveStateChanged;
    
    public UndoStack()
    {
        _stack = new(16);
        _index = -1;
        _saveIndex = -1;
    }

    public void MarkSavePoint()
    {
        _saveIndex = _index;

        UpdateSaveState();
    }

    public void Push(IOperation operation)
    {
        // If the index isn't at the top of the undo stack, we're in the middle
        // of a set of undo operations.
        if (_index < _stack.Count - 1)
        {
            // Take the existing upper section of the undo stack above the index,
            // reverse the order and invert it so that we don't lose any history
            // (emacs-style implementation).
            var redoOperations = _stack.GetRange(_index + 1, _stack.Count - _index - 1);

            for (var i = redoOperations.Count - 1; i >= 0; --i)
            {
                _stack.Add(redoOperations[i].Invert());
            }
        }
        
        // Now set the index to the top of the undo stack.
        _index = _stack.Count - 1;

        // And push the new operation.
        _stack.Add(operation);
        ++_index;

        // Clear out any operations that are above the capacity of the undo stack.
        while (_stack.Count > Capacity)
        {
            _stack.RemoveAt(0);
            --_index;
            --_saveIndex;
        }
        
        UpdateSaveState();
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
        
        UpdateSaveState();
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
        
        UpdateSaveState();
    }

    public void Clear()
    {
        _stack.Clear();
        _index = -1;
        _saveIndex = -1;

        UpdateSaveState();
    }

    private void UpdateSaveState()
    {
        var newSaveState = _index == _saveIndex;
        
        if (_saveState != newSaveState)
        {
            _saveState = newSaveState;
            SaveStateChanged?.Invoke();
        }
    }
}
