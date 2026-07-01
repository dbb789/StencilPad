using StencilPad.Spatial;

namespace StencilPad.ViewModels.Dialogs;

public class UnitScaleDialogViewModel : ViewModelBase
{
    private int _numerator;
    public int Numerator
    {
        get => _numerator;
        set => SetProperty(ref _numerator, Math.Max(1, value));
    }

    private int _denominator;
    public int Denominator
    {
        get => _denominator;
        set => SetProperty(ref _denominator, Math.Max(1, value));
    }

    public Fraction Fraction => new Fraction(Numerator, Denominator);

    public UnitScaleDialogViewModel(Fraction current)
    {
        _numerator = current.Numerator;
        _denominator = current.Denominator;
    }
}
