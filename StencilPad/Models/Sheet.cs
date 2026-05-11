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
    
    public SheetElementList Elements { get; }
    public SheetSelection Selection { get; }

    public Sheet()
    {
        Elements = new();
        Selection = new SheetSelection(Elements);
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
}
