using System.Windows.Input;

namespace StencilPad.Common;

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T>? _canExecute;

    public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null) return true;
        
        if (parameter is null && typeof(T).IsValueType) return _canExecute(default!);
        
        return parameter is T t && _canExecute(t);
    }

    public void Execute(object? parameter)
    {
        if (parameter is T t)
        {
            _execute(t);
        }
        else if (parameter is null && !typeof(T).IsValueType)
        {
            _execute(default!);
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}

public class RelayCommand : RelayCommand<object?>
{
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        : base(execute, canExecute)
    {
    }

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : base(_ => execute(), canExecute == null ? null : _ => canExecute())
    {
    }
}
