using System.ComponentModel;
using System.Runtime.CompilerServices;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class Project : INotifyPropertyChanged
{
    public IEnumerable<Sheet> Sheets => _sheets.Values;
    
    private Dictionary<Guid, Sheet> _sheets;

    public event Action<Sheet>? SheetAdded;
    public event Action<Sheet>? SheetRemoved;
    public event PropertyChangedEventHandler? PropertyChanged;

    private UnitSystem _unitSystem = UnitSystem.Metric;
    public UnitSystem UnitSystem
    {
        get => _unitSystem;
        set
        {
            if (_unitSystem != value)
            {
                _unitSystem = value;
                OnPropertyChanged();
            }
        }
    }

    private Fraction _unitRatio = Fraction.One;
    public Fraction UnitRatio
    {
        get => _unitRatio;
        set
        {
            if (_unitRatio != value)
            {
                _unitRatio = value;
                OnPropertyChanged();
            }
        }
    }
    
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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
