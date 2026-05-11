using System.Collections;
using System.Collections.Specialized;

namespace StencilPad.Models;

public class SheetElementList : IEnumerable<ISheetElement>, INotifyCollectionChanged
{
    public struct Enumerator : IEnumerator<ISheetElement>
    {
        public ISheetElement Current => _parent._elements[_parent._keys[_index]];
        object IEnumerator.Current => _parent._elements[_parent._keys[_index]];

        private readonly SheetElementList _parent;
        private readonly int _version;
        private int _index;
        
        public Enumerator(SheetElementList parent)
        {
            _parent = parent;
            _version = _parent._version;
            _index = -1;
        }

        public bool MoveNext()
        {
            if (_version != _parent._version)
            {
                throw new InvalidOperationException("Collection was modified during enumeration.");
            }
            
            return ++_index < _parent._keys.Count;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
            // ..
        }
    }

    private readonly List<Guid> _keys;
    private readonly Dictionary<Guid, ISheetElement> _elements;
    private int _version;
    
    // NOTE: Strictly defined to be called before CollectionChanged so that the
    // selection in SheetSelection can be updated before everything else is
    // notified.
    public event Action<ISheetElement>? ElementRemoving;
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public SheetElementList()
    {
        _keys = new();
        _elements = new();
        _version = 0;
    }

    public bool TryGetElement(Guid id, out ISheetElement element)
    {
        return _elements.TryGetValue(id, out element!);
    }

    public void Add(ISheetElement element)
    {
        var id = element.Id;
        
        if (_elements.ContainsKey(id))
        {
            throw new ArgumentException($"Element with Id {element.Id} already exists in the list.");
        }
        
        _keys.Add(id);
        _elements[id] = element;
        ++_version;
        
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                                                                             element,
                                                                             _keys.Count - 1));
    }

    public void Remove(ISheetElement element)
    {
        var id = element.Id;

        if (!_elements.TryGetValue(id, out var existingElement))
        {
            throw new ArgumentException($"Element with Id {id} does not exist in the list.");
        }

        ElementRemoving?.Invoke(existingElement);
        
        // Safety - keep element around until after ElementRemoving is invoked.
        _elements.Remove(id);
        
        // O(n) but shouldn't be a real-world problem.
        int keyIndex = _keys.IndexOf(id);
        
        _keys.RemoveAt(keyIndex);
        ++_version;

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, existingElement, keyIndex));
    }

    public void Clear()
    {
        foreach (var element in _elements.Values)
        {
            ElementRemoving?.Invoke(element);
        }
        
        _keys.Clear();
        _elements.Clear();
        ++_version;
        
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
    
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<ISheetElement> IEnumerable<ISheetElement>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }
}
