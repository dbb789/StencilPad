namespace StencilPad.Spatial;

public class ObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly Stack<T> _pool;

    public ObjectPool(int initalCapacity = 0)
    {
        _pool = new Stack<T>(initalCapacity);
    }

    public T? TryGet()
    {
        return (_pool.Count > 0) ? _pool.Pop() : null;
    }

    public void Recycle(T obj)
    {
        _pool.Push(obj);
    }
}
