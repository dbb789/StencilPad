using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StencilPad.Models;

public abstract class ModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; protected set; }

    public ModelBase()
    {
        Id = Guid.NewGuid();
    }

    public void NewId()
    {
        Id = Guid.NewGuid();
    }
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
