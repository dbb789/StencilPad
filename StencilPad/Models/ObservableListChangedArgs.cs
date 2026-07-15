namespace StencilPad.Models;

// CollectionChangedEventArgs is bloated, allocates on the heap, and isn't type
// safe. Although non standard, this wildly simplifies everything else, should
// be more efficient, and should be a little less likely to misuse.
public record struct ObservableListChangedArgs<T>
{
    public static ObservableListChangedArgs<T> Add(T item, int index) =>
        new ObservableListChangedArgs<T>(ObservableListChangedAction.Add, item, index, index);

    public static ObservableListChangedArgs<T> Remove(T item) =>
        new ObservableListChangedArgs<T>(ObservableListChangedAction.Remove, item, -1, -1);

    public static ObservableListChangedArgs<T> Move(T item, int oldIndex, int newIndex) =>
        new ObservableListChangedArgs<T>(ObservableListChangedAction.Move, item, oldIndex, newIndex);

    public ObservableListChangedAction Action { get; init; }
    public T Item { get; init; }
    public int OldIndex { get; init; }
    public int NewIndex { get; init; }

    private ObservableListChangedArgs(ObservableListChangedAction action,
                                      T item,
                                      int oldIndex,
                                      int newIndex)
    {
        Action = action;
        Item = item;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }
}
