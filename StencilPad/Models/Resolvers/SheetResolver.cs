using System.Collections;
using Microsoft.Extensions.Logging;
using StencilPad.Common;
using System.Collections.Specialized;
using StencilPad.Collections;

namespace StencilPad.Models.Resolvers;

public class SheetResolver : IDisposable
{
    public class Factory(ILogger<SheetResolver> Logger,
                         ISettings Settings,
                         IResourceSet ResourceSet)
    {
        public SheetResolver Create()
        {
            return new(Logger, Settings, ResourceSet);
        }

        public SheetResolver Create(Sheet sheet)
        {
            return new(Logger, sheet, Settings, ResourceSet);
        }
    }
    
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

    private readonly ILogger<SheetResolver> _logger;
    private readonly ISettings _settings;
    private readonly IResourceSet _resourceSet;
    private readonly OrderedDictionary<ISheetElement, ISheetElementResolver> _resolvers = new();
    private Sheet? _sheet;
    private int _version;

    public event Action<ObservableListChangedArgs<ISheetElementResolver>>? ElementsChanged;
    public event Action<ObservableListChangedArgs<ISheetElementResolver>>? SelectionChanged;

    private SheetResolver(ILogger<SheetResolver> logger,
                          ISettings settings,
                          IResourceSet resourceSet)
    {
        _logger = logger;
        _settings = settings;
        _resourceSet = resourceSet;
    }
    
    private SheetResolver(ILogger<SheetResolver> logger,
                          Sheet sheet,
                          ISettings settings,
                          IResourceSet resourceSet)
    {
        _logger = logger;
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
            foreach (var element in _sheet.Selection)
            {
                RemoveSelection(element);
            }

            foreach (var element in _sheet.Elements)
            {
                RemoveResolver(element);
            }
            
            _sheet.Elements.ListChanged -= OnElementsChanged;
            _sheet.Selection.ListChanged -= OnSelectionChanged;
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
            foreach (var element in _sheet.Elements)
            {
                AddResolver(element);
            }

            int index = 0;
            
            foreach (var element in _sheet.Selection)
            {
                AddSelection(element, index++);
            }

            _sheet.Elements.ListChanged += OnElementsChanged;
            _sheet.Selection.ListChanged += OnSelectionChanged;
        }
    }

    private void OnElementsChanged(ObservableListChangedArgs<ISheetElement> e)
    {
        switch (e.Action)
        {
        case ObservableListChangedAction.Add:
            AddResolver(e.Item, e.NewIndex);
            break;
            
        case ObservableListChangedAction.Remove:
            RemoveResolver(e.Item);
            break;
            
        case ObservableListChangedAction.Move:
            MoveResolver(e.OldIndex, e.NewIndex);
            break;
        }
    }

    private void OnSelectionChanged(ObservableListChangedArgs<ISheetElement> e)
    {
        switch (e.Action)
        {
        case ObservableListChangedAction.Add:
            AddSelection(e.Item, e.NewIndex);
            break;
            
        case ObservableListChangedAction.Remove:
            RemoveSelection(e.Item);
            break;
        }
    }

    private void AddResolver(ISheetElement element, int index = -1)
    {
        if (index < 0)
        {
            index = _resolvers.Count;
        }

        var resolver = ResolverFactory.Create(element, _settings, _resourceSet);

        if (resolver is null)
        {
            _logger.LogError("Could not create resolver for element of type {SheetElement}", element.GetType().Name);
            return;
        }
        
        _resolvers.Insert(index, element, resolver);
        
        ++_version;

        ElementsChanged?.Invoke(ObservableListChangedArgs<ISheetElementResolver>.Add(resolver, index));
    }

    private void RemoveResolver(ISheetElement element)
    {
        if (_resolvers.TryGetValue(element, out var resolver))
        {
            resolver.Dispose();
            _resolvers.Remove(element);
            
            ++_version;
            ElementsChanged?.Invoke(ObservableListChangedArgs<ISheetElementResolver>.Remove(resolver));
        }
        else
        {
            _logger.LogError("Could not find resolver for element of type {SheetElement}", element.GetType().Name);
        }
    }

    private void MoveResolver(int prevIndex, int newIndex)
    {
        var kvp = _resolvers.GetAt(prevIndex);
        
        _resolvers.RemoveAt(prevIndex);
        _resolvers.Insert(newIndex, kvp.Key, kvp.Value);
        
        ++_version;
        ElementsChanged?.Invoke(ObservableListChangedArgs<ISheetElementResolver>.Move(kvp.Value, prevIndex, newIndex));
    }

    private void AddSelection(ISheetElement element, int index)
    {
        if (_resolvers.TryGetValue(element, out var resolver))
        {
            SelectionChanged?.Invoke(ObservableListChangedArgs<ISheetElementResolver>.Add(resolver, index));
        }
        else
        {
             _logger.LogError("Could not find resolver for selected element of type {SheetElement}", element.GetType().Name);
        }
    }

    private void RemoveSelection(ISheetElement element)
    {
        if (_resolvers.TryGetValue(element, out var resolver))
        {
            SelectionChanged?.Invoke(ObservableListChangedArgs<ISheetElementResolver>.Remove(resolver));
        }
        else
        {
            _logger.LogError("Could not find resolver for selected element of type {SheetElement}", element.GetType().Name);
        }
    }
}

