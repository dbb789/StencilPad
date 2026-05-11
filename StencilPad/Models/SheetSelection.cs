using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace StencilPad.Models;

// Tracks a selection of sheet elements by ID based on a parent
// ObservableCollection, but exposes selected elements as ISheetElement.
public class SheetSelection : IEnumerable<ISheetElement>, INotifyCollectionChanged
{
    public struct Enumerator : IEnumerator<ISheetElement>
    {
        ISheetElement IEnumerator<ISheetElement>.Current => _current;
        object IEnumerator.Current => _current;

        private readonly Collection<ISheetElement> _elements;
        private readonly HashSet<Guid> _selectedIds;
        private HashSet<Guid>.Enumerator _enumerator;
        private ISheetElement _current;

        public Enumerator(Collection<ISheetElement> elements,
                          HashSet<Guid> selectedIds)
        {
            _elements = elements;
            _selectedIds = selectedIds;
            _enumerator = _selectedIds.GetEnumerator();
            _current = null!;
        }

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                var id = _enumerator.Current;
                var element = _elements.Where(e => e.Id == id).FirstOrDefault();

                if (element is not null)
                {
                    _current = element;

                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            _enumerator = _selectedIds.GetEnumerator();
            _current = null!;
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }

    public int Count => _selectedIds.Count;
    
    private readonly ObservableCollection<ISheetElement> _elements;
    private readonly HashSet<Guid> _selectedIds;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    
    public SheetSelection(ObservableCollection<ISheetElement> elements)
    {
        _elements = elements;
        _selectedIds = new();
        
        _elements.CollectionChanged += OnElementsChanged;
    }

    public void Add(ISheetElement element)
    {
        _selectedIds.Add(element.Id);

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, element));
    }

    public void Remove(ISheetElement element)
    {
        _selectedIds.Remove(element.Id);

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, element));
    }

    public void Clear()
    {
        _selectedIds.Clear();

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void OnElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is null)
        {
            return;
        }

        foreach (ISheetElement removed in e.OldItems)
        {
            Remove(removed);
        }
    }
    
    IEnumerator<ISheetElement> IEnumerable<ISheetElement>.GetEnumerator()
    {
        return new Enumerator(_elements, _selectedIds);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(_elements, _selectedIds);
    }
}
