using System.Collections;

namespace StencilPad.Models.Resolvers;

public struct SheetResolverEnumerator<TEnumerator> : IEnumerator<ISheetElementResolver>, IEnumerator
    where TEnumerator : struct, IEnumerator<ISheetElement>
{
    public ISheetElementResolver Current => _current!;
    object IEnumerator.Current => _current!;

    private readonly SheetResolver _parent;
    private TEnumerator _enumerator;
    private ISheetElementResolver? _current;

    public SheetResolverEnumerator(SheetResolver parent, TEnumerator enumerator)
    {
        _parent = parent;
        _enumerator = enumerator;
        _current = default;
    }

    public bool MoveNext()
    {
        if (_parent is null)
        {
            return false;
        }

        while (_enumerator.MoveNext())
        {
            if (_parent.TryGetResolver(_enumerator.Current, out var resolver))
            {
                _current = resolver;

                return true;
            }
        }

        return false;
    }

    public void Reset()
    {
        _enumerator.Reset();
        _current = default;
    }

    public void Dispose()
    {
        _enumerator.Dispose();
    }
}
