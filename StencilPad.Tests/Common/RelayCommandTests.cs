namespace StencilPad.Tests.Common;

using System.Windows.Input;
using StencilPad.Common;

public class RelayCommandTests
{
    // --- Parameterless RelayCommand ---

    [Test]
    public void Execute_Parameterless_InvokesAction()
    {
        bool invoked = false;
        var command = new RelayCommand(() => invoked = true);

        // Should ignore parameter entirely
        command.Execute(null);

        Assert.That(invoked, Is.True);
    }

    [Test]
    public void CanExecute_Parameterless_ReturnsTrue()
    {
        var command = new RelayCommand(() => { });

        Assert.That(command.CanExecute(null), Is.True);
        Assert.That(command.CanExecute("test"), Is.True);
    }

    // --- Generic RelayCommand<T> ---

    [Test]
    public void Execute_Generic_WithCorrectType_InvokesAction()
    {
        string? passedParam = null;
        var command = new RelayCommand<string>(p => passedParam = p);

        command.Execute("test");

        Assert.That(passedParam, Is.EqualTo("test"));
    }

    [Test]
    public void Execute_Generic_WithIncorrectType_ThrowsArgumentException()
    {
        var command = new RelayCommand<int>(p => { });

        Assert.That(() => command.Execute("not an int"), Throws.ArgumentException);
    }

    [Test]
    public void Execute_Generic_WithNull_ThrowsArgumentException()
    {
        // Due to strict `parameter is T t` checking, even nullable reference types fail if null is passed.
        var command = new RelayCommand<string?>(p => { });

        Assert.That(() => command.Execute(null), Throws.ArgumentException);
    }

    [Test]
    public void CanExecute_Generic_WithCorrectType_ReturnsTrue()
    {
        var command = new RelayCommand<string>(p => { });

        Assert.That(command.CanExecute("test"), Is.True);
    }

    [Test]
    public void CanExecute_Generic_WithIncorrectType_ReturnsFalse()
    {
        var command = new RelayCommand<int>(p => { });

        Assert.That(command.CanExecute("test"), Is.False);
    }

    [Test]
    public void CanExecute_Generic_WithNull_ReturnsFalse()
    {
        // `is T` returns false for nulls, regardless of T's nullability.
        var command = new RelayCommand<string?>(p => { });

        Assert.That(command.CanExecute(null), Is.False);
    }
}
