namespace StencilPad.Models;

public class Project
{
    public IEnumerable<Sheet> Sheets => _sheets.Values;
    
    private Dictionary<Guid, Sheet> _sheets;

    public event Action<Sheet>? SheetAdded;
    public event Action<Sheet>? SheetRemoved;
    
    public Project()
    {
        _sheets = [];
    }

    public bool TryGetSheet(Guid guid, out Sheet sheet)
    {
        return _sheets.TryGetValue(guid, out sheet!);
    }

    public void AddSheet(Sheet sheet)
    {
        if (_sheets.ContainsKey(sheet.Id))
        {
            throw new InvalidOperationException($"A sheet with ID {sheet.Id} already exists in the project.");
        }

        _sheets.Add(sheet.Id, sheet);
        SheetAdded?.Invoke(sheet);
    }

    public void RemoveSheet(Sheet sheet)
    {
        if (_sheets.Remove(sheet.Id))
        {
            SheetRemoved?.Invoke(sheet);
        }
        else
        {
            throw new InvalidOperationException($"No sheet with ID {sheet.Id} exists in the project.");
        }
    }
    
    public void Clear()
    {
        foreach (var sheet in _sheets.Values.ToList())
        {
            RemoveSheet(sheet);
        }
    }
}
