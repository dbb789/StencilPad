namespace StencilPad.Collections;

public interface IKeyedList<T>
{
    T this[Index index] { get; set; }
    int Count { get; }

    event Action<int, ulong, T, T>? ItemReassigned;
    
    T At(int index);
    int IndexOfKey(ulong key);
    T GetByKey(ulong key);
    ulong KeyAt(int index);
}
