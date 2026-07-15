namespace StencilPad.Collections;

public interface IObservableList<T>
{
    event Action<ObservableListChangedArgs<T>>? ListChanged;
}
