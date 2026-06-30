using System.Collections;
using System.Collections.Specialized;
using StencilPad.Common;

namespace StencilPad.Models.Resolvers;

public class SheetResolver : IDisposable
{
    public struct Enumerator : IEnumerator<(ISheetElement Element, IModelResolver Resolver)>
    {
        public (ISheetElement Element, IModelResolver Resolver) Current => _current;
        object IEnumerator.Current => _current;

        private readonly SheetResolver _parent;
        private readonly int _version;
        private int _index;
        private (ISheetElement, IModelResolver) _current;

        public Enumerator(SheetResolver parent)
        {
            _parent = parent;
            _version = parent._version;
            _index = -1;
            _current = default;
        }

        public bool MoveNext()
        {
            if (_version != _parent._version)
            {
                throw new InvalidOperationException("Collection was modified during enumeration.");
            }

            var elements = _parent._sheet?.Elements;

            if (elements is null)
            {
                return false;
            }

            while (++_index < elements.Count)
            {
                var element = elements[_index];

                if (_parent._resolvers.TryGetValue(element, out var resolver))
                {
                    _current = (element, resolver);
                    
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
            _current = default;
        }

        public void Dispose()
        {
            // ..
        }
    }

    private readonly ISettings _settings;
    private readonly IResourceSet _resourceSet;
    private Sheet? _sheet;
    private readonly OrderedDictionary<ISheetElement, IModelResolver> _resolvers = new();
    private int _version;

    public event Action<ISheetElement, IModelResolver>? ResolverAdded;
    public event Action<ISheetElement, IModelResolver>? ResolverRemoved;

    public SheetResolver(ISettings settings,
                         IResourceSet resourceSet)
    {
        _settings = settings;
        _resourceSet = resourceSet;
    }
    
    public SheetResolver(Sheet sheet,
                         ISettings settings,
                         IResourceSet resourceSet)
    {
        _settings = settings;
        _resourceSet = resourceSet;

        SetSheet(sheet);
    }

    public Sheet? Sheet
    {
        get => _sheet;
        set => SetSheet(value);
    }

    public bool TryGetResolver(ISheetElement element, out IModelResolver? resolver)
    {
        return _resolvers.TryGetValue(element, out resolver);
    }

    public void Dispose()
    {
        SetSheet(null);
    }

    private void SetSheet(Sheet? sheet)
    {
        if (_sheet == sheet)
        {
            return;
        }

        if (_sheet is not null)
        {
            _sheet.Elements.CollectionChanged -= OnElementsChanged;
        }

        foreach (var resolver in _resolvers.Values)
        {
            resolver.Dispose();
        }

        _resolvers.Clear();
        
        ++_version;

        _sheet = sheet;

        if (_sheet is not null)
        {
            _sheet.Elements.CollectionChanged += OnElementsChanged;

            foreach (var element in _sheet.Elements)
            {
                AddResolver(element);
            }
        }
    }

    private void OnElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is ISheetElement element)
                {
                    RemoveResolver(element);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is ISheetElement element)
                {
                    AddResolver(element, e.NewStartingIndex);
                }
            }
        }
    }

    private void AddResolver(ISheetElement element, int index = -1)
    {
        var resolver = ResolverFactory.Create(element, _settings, _resourceSet);

        if (resolver is null)
        {
            return;
        }

        if (index < 0)
        {
            index = _resolvers.Count;
        }

        _resolvers.Insert(index, element, resolver);
        
        ++_version;
        
        ResolverAdded?.Invoke(element, resolver);
    }

    private void RemoveResolver(ISheetElement element)
    {
        if (_resolvers.TryGetValue(element, out var resolver))
        {
            resolver.Dispose();
            _resolvers.Remove(element);
            
            ++_version;
            
            ResolverRemoved?.Invoke(element, resolver);
        }
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }
}

