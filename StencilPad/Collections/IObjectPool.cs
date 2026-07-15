namespace StencilPad.Collections;

public interface IObjectPool<T>
{
    T? TryGet();
    void Recycle(T obj);
}
