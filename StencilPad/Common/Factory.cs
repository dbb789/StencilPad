namespace StencilPad.Common;

public class Factory<T>(Func<T> create)
{
    public T Create() => create();
}
