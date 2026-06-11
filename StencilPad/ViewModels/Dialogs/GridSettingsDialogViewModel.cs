using StencilPad.Spatial;

namespace StencilPad.ViewModels.Dialogs;

public class GridSettingsDialogViewModel : ViewModelBase
{
    private Unit _spacing;
    public Unit Spacing
    {
        get => _spacing;
        set => SetProperty(ref _spacing, value);
    }

    private int _subdivisions;
    public int Subdivisions
    {
        get => _subdivisions;
        set => SetProperty(ref _subdivisions, value);
    }

    // Spacing is currently defined in terms of sheet size units, so we use a
    // default unit settings with a scale of 1. If we don't do this then
    // changing unit ratio in the main UI will make the grid resize all over the
    // place.
    //
    // At some point we might want to display what the scaled spacing is after
    // the unit ratio has been applied to it.
    public UnitSettings UnitSettings
    {
        get
        {
            return new UnitSettings(_unitSystem, Fraction.One);
        }
    }

    private readonly UnitSystem _unitSystem;

    public GridSettingsDialogViewModel(Unit currentSpacing,
                                       int currentSubdivisions,
                                       UnitSettings unitSettings)
    {
        _unitSystem = unitSettings.System;
        _spacing = currentSpacing;
        _subdivisions = currentSubdivisions;
    }
}
