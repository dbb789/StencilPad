using System.Collections;
using System.Collections.Specialized;

namespace StencilPad.Models;

public class SheetElementList : IEnumerable<ISheetElement>, INotifyCollectionChanged
{
    public struct Enumerator : IEnumerator<ISheetElement>
    {
        public ISheetElement Current => _elements[_keys[_index]];
        ISheetElement IEnumerator<ISheetElement>.Current => _elements[_keys[_index]];
        object IEnumerator.Current => _elements[_keys[_index]];

        private readonly List<Guid> _keys;
        private readonly Dictionary<Guid, ISheetElement> _elements;
        private int _index;
        
        public Enumerator(List<Guid> keys,
                          Dictionary<Guid, ISheetElement> elements)
        {
            _keys = keys;
            _elements = elements;
            _index = -1;
        }

        public bool MoveNext()
        {
            return ++_index < _keys.Count;
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

    private List<Guid> _keys;
    private Dictionary<Guid, ISheetElement> _elements;

    // NOTE: Strictly defined to be called before CollectionChanged so that the
    // selection in SheetSelection can be updated before everything else is
    // notified.
    public event Action<ISheetElement>? ElementRemoving;
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public SheetElementList()
    {
        _keys = new();
        _elements = new();
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

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, element));
    }

    public void Remove(ISheetElement element)
    {
        var id = element.Id;
        
        if (!_elements.ContainsKey(id))
        {
            throw new ArgumentException($"Element with Id {id} does not exist in the list.");
        }

        ElementRemoving?.Invoke(element);
        
        // Safety - keep element around until after ElementRemoving is invoked.
        _elements.Remove(id);
        
        // O(n) but shouldn't be a real-world problem.
        _keys.Remove(id);

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, element));
    }

    public void Clear()
    {
        foreach (var element in _elements.Values)
        {
            ElementRemoving?.Invoke(element);
        }
        
        _keys.Clear();
        _elements.Clear();
        
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
    
    public Enumerator GetEnumerator()
    {
        return new Enumerator(_keys, _elements);
    }

    IEnumerator<ISheetElement> IEnumerable<ISheetElement>.GetEnumerator()
    {
        return new Enumerator(_keys, _elements);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(_keys, _elements);
    }
}
