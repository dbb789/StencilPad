using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace StencilPad.Models;

public class Sheet : ModelBase
{
    private string _name = "Sheet";
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }
    
    public ObservableCollection<ISheetElement> Elements { get; } = [];
    public ObservableCollection<ISheetElement> Selection { get; } = [];

    public Sheet()
    {
        Elements.CollectionChanged += OnElementsChanged;
    }

    public bool AssignElement(ISheetElement newElement)
    {
        var element = Elements.Where(e => e.Id == newElement.Id).First();

        if (element is not null)
        {
            element.AssignFromElement(newElement);

            return true;
        }

        return false;
    }

    public bool RemoveElement(Guid Id)
    {
        var element = Elements.Where(e => e.Id == Id).FirstOrDefault();

        if (element is not null)
        {
            Elements.Remove(element);
            
            return true;
        }

        return false;
    }
    
    private void OnElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is null)
        {
            return;
        }

        foreach (SheetElement removed in e.OldItems)
        {
            Selection.Remove(removed);
        }
    }
}
