using System.ComponentModel;
using System.Windows.Media;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Services;

public class SettingsService : ISettings
{
    public UnitSystem UnitSystem => _project?.UnitSystem ?? UnitSystem.Metric;
    public Fraction UnitRatio => _project?.UnitRatio ?? Fraction.One;
    public UnitSettings UnitSettings => _project?.UnitSettings ?? UnitSettings.Default;
    
    public Color GridLineColor => _appConfigService.Config.GridLineColor;
    public Color SelectionColor => _appConfigService.Config.SelectionColor;
    public Color GroupSelectionColor => _appConfigService.Config.GroupSelectionColor;
    public Color MoveHandleColor => _appConfigService.Config.MoveHandleColor;
    public Color AdjustHandleColor => _appConfigService.Config.AdjustHandleColor;
    
    public double HandleSizePx => _appConfigService.Config.HandleSizePx;
    public double PointSnapPx => _appConfigService.Config.PointSnapPx;

    public Unit GridSpacing
    {
        get
        {
            if (_project is null)
            {
                return Unit.FromMillimeters(10);
            }

            return (UnitSystem == UnitSystem.Metric) ?
                _project.GridSpacingMetric : _project.GridSpacingImperial;
        }
    }

    public int GridSubdivisions
    {
        get
        {
            if (_project is null)
            {
                return 5;
            }

            return (UnitSystem == UnitSystem.Metric) ?
                _project.GridSubdivisionsMetric : _project.GridSubdivisionsImperial;
        }
    }
    
    public double GridMinSpacingPx => _appConfigService.Config.GridMinSpacingPx;

    private readonly IAppConfigService _appConfigService;
    
    public event Action? Changed;

    private Project? _project;
    public Project? Project
    {
        get => _project;
        set
        {
            SetProject(value);
        }
    }

    public SettingsService(IAppConfigService appConfigService,
                           Project? project = null)
    {
        _appConfigService = appConfigService;
        _appConfigService.Applied += InvokeChanged;

        SetProject(project);
    }

    private void SetProject(Project? project)
    {
        if (_project == project)
        {
            return;
        }

        if (_project is not null)
        {
            _project.PropertyChanged -= ProjectPropertyChanged;
        }

        _project = project;

        if (_project is not null)
        {
            _project.PropertyChanged += ProjectPropertyChanged;
        }
    }

    private void ProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvokeChanged();
    }
    
    private void InvokeChanged()
    {
        Changed?.Invoke();
    }
}
