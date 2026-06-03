using StencilPad.Services;

namespace StencilPad.Models.Resolvers;

public class GroupResolver : IModelResolver
{
    private readonly ElementGroup _group;
    private readonly IResourceService _resourceService;
    private readonly List<(IModelResolver, IModelWalker)> _children;

    private IModelWalker? _walker;

    public GroupResolver(ElementGroup group, IResourceService resourceService)
    {
        _group = group;
        _resourceService = resourceService;
        _children = new();

        _group.TransformChanged += TransformChanged;
    }

    public void Dispose()
    {
        Detach();
        
        _group.TransformChanged -= TransformChanged;
    }

    public void Attach(IModelWalker walker)
    {
        _walker = walker;
        
        foreach (var element in _group.Children)
        {
            var childResolver = ResolverFactory.Create(element, _resourceService);

            if (childResolver is not null)
            {
                var childWalker = _walker.CreateModelWalker(childResolver);

                _children.Add((childResolver, childWalker));
            }
        }
    }

    public void Detach()
    {
        foreach (var (childResolver, childWalker) in _children)
        {
            childResolver.Detach();
            childWalker.Dispose();
        }

        _children.Clear();
        _walker = null;
    }

    private void TransformChanged(ISheetElement element)
    {
        _walker?.SetTransform(_group.Transform);
    }
}
