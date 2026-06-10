using System.ComponentModel;
using System.Windows.Media;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Services;

public class SettingsService : ISettings
{
    public UnitType UnitType => _project?.UnitType ?? UnitType.Metric;
    
    public Color GridLineColor => _appConfigService.Config.GridLineColor;
    public Color SelectionColor => _appConfigService.Config.SelectionColor;
    public Color GroupSelectionColor => _appConfigService.Config.GroupSelectionColor;
    public Color MoveHandleColor => _appConfigService.Config.MoveHandleColor;
    public Color AdjustHandleColor => _appConfigService.Config.AdjustHandleColor;
    
    public double HandleSizePx => _appConfigService.Config.HandleSizePx;
    public double PointSnapPx => _appConfigService.Config.PointSnapPx;

    public Unit GridSpacing => (UnitType == UnitType.Metric) ?
        _appConfigService.Config.GridSpacingMetric : _appConfigService.Config.GridSpacingImperial;
    
    public int GridSubdivisions => (UnitType == UnitType.Metric) ?
        _appConfigService.Config.GridSubdivisionsMetric : _appConfigService.Config.GridSubdivisionsImperial;
    
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
