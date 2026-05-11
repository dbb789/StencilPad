using System.Collections;
using System.Collections.Specialized;

namespace StencilPad.Models;

// Tracks a selection of sheet elements by ID based on a parent
// ObservableCollection, but exposes selected elements as ISheetElement.
public class SheetSelection : IEnumerable<ISheetElement>, INotifyCollectionChanged
{
    public struct Enumerator : IEnumerator<ISheetElement>
    {
        public ISheetElement Current => _current;
        object IEnumerator.Current => _current;

        private readonly SheetSelection _parent;
        private readonly int _version;
        private HashSet<Guid>.Enumerator _enumerator;
        private ISheetElement _current;

        public Enumerator(SheetSelection parent)
        {
            _parent = parent;
            _version = _parent._version;
            _enumerator = _parent._selectedIds.GetEnumerator();
            _current = null!;
        }

        public bool MoveNext()
        {
            if (_version != _parent._version)
            {
                throw new InvalidOperationException("Collection was modified during enumeration.");
            }

            while (_enumerator.MoveNext())
            {
                var id = _enumerator.Current;
                
                if (_parent._elements.TryGetElement(id, out var element))
                {
                    _current = element;

                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            _enumerator = _parent._selectedIds.GetEnumerator();
            _current = null!;
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }

    public int Count => _selectedIds.Count;
    
    private readonly SheetElementList _elements;
    private readonly HashSet<Guid> _selectedIds;
    private int _version;
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    
    public SheetSelection(SheetElementList elements)
    {
        _elements = elements;
        _selectedIds = new();
        _version = 0;

        _elements.ElementRemoving += (e) => Remove(e);
    }

    public bool Add(ISheetElement element)
    {
        if (!_elements.TryGetElement(element.Id, out var existingElement))
        {
            throw new ArgumentException("Element does not exist in parent collection.", nameof(element));
        }
        
        if (_selectedIds.Add(element.Id))
        {
            ++_version;

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, existingElement));

            return true;
        }

        return false;
    }

    public bool Remove(ISheetElement element)
    {
        if (!_elements.TryGetElement(element.Id, out var existingElement))
        {
            throw new ArgumentException("Element does not exist in parent collection.", nameof(element));
        }
        
        if (_selectedIds.Remove(element.Id))
        {
            ++_version;

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, existingElement));

            return true;
        }

        return false;
    }

    public void Clear()
    {
        if (_selectedIds.Count == 0)
        {
            return;
        }
        
        _selectedIds.Clear();
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
