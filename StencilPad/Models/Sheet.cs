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

    private SheetFormat _format = new SheetFormat(SheetSizeType.A4,
                                                  SheetOrientation.Portrait);
    public SheetFormat Format
    {
        get => _format;
        set
        {
            if (_format != value)
            {
                _format = value;
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
        if (Elements.TryGetElement(newElement.Id, out var element))
        {
            element.AssignFromElement(newElement);

            return true;
        }

        return false;
    }
}
