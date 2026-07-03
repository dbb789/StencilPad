using System.ComponentModel;
using System.Runtime.CompilerServices;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class Project : INotifyPropertyChanged
{
    public IEnumerable<Sheet> Sheets => _sheets.Values;
    public IEnumerable<ISheetElement> DefaultElements => _defaultElements.Values;

    private Dictionary<Guid, Sheet> _sheets;
    private Dictionary<Type, ISheetElement> _defaultElements;

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

    public UnitSettings UnitSettings => new(UnitSystem, UnitRatio);
    
    public Unit _gridSpacingMetric = Unit.FromMillimeters(10);
    public Unit GridSpacingMetric
    {
        get => _gridSpacingMetric;
        set
        {
            if (_gridSpacingMetric != value)
            {
                _gridSpacingMetric = value;
                OnPropertyChanged();
            }
        }
    }

    public int _gridSubdivisionsMetric = 5;
    public int GridSubdivisionsMetric
    {
        get => _gridSubdivisionsMetric;
        set
        {
            if (_gridSubdivisionsMetric != value)
            {
                _gridSubdivisionsMetric = value;
                OnPropertyChanged();
            }
        }
    }

    public Unit _gridSpacingImperial = Unit.FromInches(1);
    public Unit GridSpacingImperial
    {
        get => _gridSpacingImperial;
        set
        {
            if (_gridSpacingImperial != value)
            {
                _gridSpacingImperial = value;
                OnPropertyChanged();
            }
        }
    }

    public int _gridSubdivisionsImperial = 8;
    public int GridSubdivisionsImperial
    {
        get => _gridSubdivisionsImperial;
        set
        {
            if (_gridSubdivisionsImperial != value)
            {
                _gridSubdivisionsImperial = value;
                OnPropertyChanged();
            }
        }
    }
    
    public Project()
    {
        _sheets = [];
        _defaultElements = [];
    }

    public void GetElementStyle<T>(T target) where T : class, ISheetElement, new()
    {
        if (!_defaultElements.TryGetValue(typeof(T), out var stored))
        {
            stored = new T();
            _defaultElements[typeof(T)] = stored;
        }
        
        target.AssignStyleFromElement(stored);
    }

    public void SetElementStyle<T>(T source) where T : class, ISheetElement, new()
    {
        if (!_defaultElements.TryGetValue(typeof(T), out var stored))
        {
            stored = new T();
            _defaultElements[typeof(T)] = stored;
        }

        stored.AssignStyleFromElement(source);
    }

    public void SetElementStyle(ISheetElement source)
    {
        var type = source.GetType();
        
        if (!_defaultElements.TryGetValue(type, out var stored))
        {
            stored = Activator.CreateInstance(type) as ISheetElement;
            
            if (stored is null)
            {
                throw new InvalidOperationException($"Could not create an instance of type {type.FullName}");
            }
            
            _defaultElements[type] = stored;
        }

        stored.AssignStyleFromElement(source);
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
