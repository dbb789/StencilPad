using StencilPad.Common;
using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public class GroupResolver : SheetElementResolver
{
    private readonly ElementGroup _group;
    private readonly ISettings _settings;
    private readonly IResourceSet _resourceSet;
    private readonly List<(ISheetElementResolver, IModelWalker)> _children;

    private IModelWalker? _walker;

    public GroupResolver(ElementGroup group,
                         ISettings settings,
                         IResourceSet resourceSet)
        : base(group)
    {
        _group = group;
        _settings = settings;
        _resourceSet = resourceSet;
        _children = new();

        _group.TransformChanged += TransformChanged;
        _group.ChildrenChanged += OnChildrenChanged;
    }

    public override void Dispose()
    {
        Detach();
        
        _group.TransformChanged -= TransformChanged;
        _group.ChildrenChanged -= OnChildrenChanged;
    }

    public override void Attach(IModelWalker walker)
    {
        _walker = walker;
        _walker.SetTransform(_group.Transform);
        
        foreach (var element in _group.Children)
        {
            AddElement(element);
        }
    }

    public override void Detach()
    {
        ClearChildren();

        _walker = null;
    }

    private void TransformChanged(ISheetElement element)
    {
        _walker?.SetTransform(_group.Transform);

        InvokeOutlineChanged();
    }

    private void OnChildrenChanged()
    {
        if (_walker is null)
        {
            return;
        }

        ClearChildren();

        foreach (var element in _group.Children)
        {
            AddElement(element);
        }

        InvokeOutlineChanged();
    }

    private void ClearChildren()
    {
        foreach (var (childResolver, childWalker) in _children)
        {
            childResolver.OutlineChanged -= InvokeOutlineChanged;
            childResolver.Dispose();
            childWalker.Dispose();
        }

        _children.Clear();
    }

    private void AddElement(ISheetElement element)
    {
        if (_walker is null)
        {
            return;
        }
        
        var childResolver = ResolverFactory.Create(element, _settings, _resourceSet);

        if (childResolver is not null)
        {
            var childWalker = _walker.CreateModelWalker();

            childResolver.OutlineChanged += InvokeOutlineChanged;
            childResolver.Attach(childWalker);
            _children.Add((childResolver, childWalker));
        }
    }
}
