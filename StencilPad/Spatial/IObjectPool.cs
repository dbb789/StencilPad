namespace StencilPad.Spatial;

public interface IObjectPool<T>
{
    T? TryGet();
    void Recycle(T obj);
}
