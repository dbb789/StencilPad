namespace StencilPad.Spatial;

public interface IKeyedList<T>
{
    T this[int index] { get; set; }
    int Count { get; }

    event Action<int, ulong, T, T>? ItemReassigned;
    
    T At(int index);
    int IndexOfKey(ulong key);
    T GetByKey(ulong key);
    ulong KeyAt(int index);
}
