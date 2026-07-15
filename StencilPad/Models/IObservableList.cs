namespace StencilPad.Models;

public interface IObservableList<T>
{
    event Action<ObservableListChangedArgs<T>>? ListChanged;
}
