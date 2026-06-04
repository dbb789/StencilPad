namespace StencilPad.Models.Resolvers;

public class GroupResolver : IModelResolver
{
    private readonly ElementGroup _group;
    private readonly IResourceSet _resourceSet;
    private readonly List<(IModelResolver, IModelWalker)> _children;

    private IModelWalker? _walker;

    public GroupResolver(ElementGroup group, IResourceSet resourceSet)
    {
        _group = group;
        _resourceSet = resourceSet;
        _children = new();

        _group.TransformChanged += TransformChanged;
        _group.ChildrenChanged += OnChildrenChanged;
    }

    public void Dispose()
    {
        Detach();
        
        _group.TransformChanged -= TransformChanged;
        _group.ChildrenChanged -= OnChildrenChanged;
    }

    public void Attach(IModelWalker walker)
    {
        _walker = walker;
        _walker.SetTransform(_group.Transform);
        
        foreach (var element in _group.Children)
        {
            AddElement(element);
        }
    }

    public void Detach()
    {
        ClearChildren();

        _walker = null;
    }

    private void TransformChanged(ISheetElement element)
    {
        _walker?.SetTransform(_group.Transform);
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
    }

    private void ClearChildren()
    {
        foreach (var (childResolver, _) in _children)
        {
            childResolver.Dispose();
        }

        _children.Clear();
    }

    private void AddElement(ISheetElement element)
    {
        if (_walker is null)
        {
            return;
        }
        
        var childResolver = ResolverFactory.Create(element, _resourceSet);

        if (childResolver is not null)
        {
            var childWalker = _walker.CreateModelWalker();

            childResolver.Attach(childWalker);
            _children.Add((childResolver, childWalker));
        }
    }
}
