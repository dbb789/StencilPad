using System.Collections;
using System.Collections.Generic;
using StencilPad.Common;
using StencilPad.Spatial;
using System.Collections.Specialized;

namespace StencilPad.Models.Resolvers;

public class SheetResolver : IDisposable
{
    public struct ElementsView(SheetResolver SheetResolver) : IEnumerable<ISheetElementResolver>
    {
        public SheetResolverEnumerator<SheetElementList.Enumerator> GetEnumerator()
        {
            return CreateEnumerator();
        }
        
        IEnumerator<ISheetElementResolver> IEnumerable<ISheetElementResolver>.GetEnumerator()
        {
            return CreateEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return CreateEnumerator();
        }

        private SheetResolverEnumerator<SheetElementList.Enumerator> CreateEnumerator()
        {
            return new(SheetResolver, SheetResolver._sheet?.Elements.GetEnumerator() ?? default);
        }
    }

    public struct SelectedView(SheetResolver SheetResolver) : IEnumerable<ISheetElementResolver>
    {
        public SheetResolverEnumerator<SheetSelection.Enumerator> GetEnumerator()
        {
            return CreateEnumerator();
        }

        IEnumerator<ISheetElementResolver> IEnumerable<ISheetElementResolver>.GetEnumerator()
        {
            return CreateEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return CreateEnumerator();
        }

        private SheetResolverEnumerator<SheetSelection.Enumerator> CreateEnumerator()
        {
            return new(SheetResolver, SheetResolver._sheet?.Selection.GetEnumerator() ?? default);
        }
    }

    public ElementsView Elements => new(this);
    public SelectedView Selection => new(this);

    private readonly ISettings _settings;
    private readonly IResourceSet _resourceSet;
    private Sheet? _sheet;
    private readonly OrderedDictionary<ISheetElement, ISheetElementResolver> _resolvers = new();
    private int _version;

    public event Action<ISheetElementResolver>? ElementAdded;
    public event Action<ISheetElementResolver>? ElementRemoved;
    public event Action<ISheetElementResolver>? SelectionAdded;
    public event Action<ISheetElementResolver>? SelectionRemoved;

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

    public bool TryGetResolver(ISheetElement element, out ISheetElementResolver resolver)
    {
        return _resolvers.TryGetValue(element, out resolver!);
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
            _sheet.Selection.CollectionChanged -= OnSelectionChanged;
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

            _sheet.Selection.CollectionChanged += OnSelectionChanged;
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

    private void OnSelectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is ISheetElement element)
                {
                    if (_resolvers.TryGetValue(element, out var resolver))
                    {
                        SelectionRemoved?.Invoke(resolver);
                    }
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is ISheetElement element)
                {
                    if (_resolvers.TryGetValue(element, out var resolver))
                    {
                        SelectionAdded?.Invoke(resolver);
                    }
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
        
        ElementAdded?.Invoke(resolver);
    }

    private void RemoveResolver(ISheetElement element)
    {
        if (_resolvers.TryGetValue(element, out var resolver))
        {
            resolver.Dispose();
            _resolvers.Remove(element);
            
            ++_version;
            
            ElementRemoved?.Invoke(resolver);
        }
    }
}

