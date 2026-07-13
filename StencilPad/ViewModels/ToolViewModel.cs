using System.Windows.Input;
using System.Windows.Media.Imaging;
using StencilPad.Canvases.Tools.Overlays;

namespace StencilPad.ViewModels;

public class ToolViewModel : ViewModelBase, IToolButton
{
    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }
    
    public BitmapImage? Icon { get; set; }
    
    private string _tooltip = "";
    public string Tooltip
    {
        get => _tooltip;
        set => SetProperty(ref _tooltip, value);
    }
}
