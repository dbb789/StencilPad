using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;

namespace StencilPad.ViewModels.Properties;

public class ImagePropertiesViewModel : ElementPropertiesViewModel<ImageElement>
{
    public string Title => "Image Properties";

    private double _opacity;
    public double Opacity
    {
        get => _opacity;
        set
        {
            _opacity = value;
            SetElementProperty(e => e.Opacity = value);
            OnPropertyChanged();
        }
    }

    private IDisposable? _dragContext;

    public ImagePropertiesViewModel(Sheet sheet,
                                    ISettings settings,
                                    IOperationService operationService)
        : base(sheet, operationService, settings)
    {
        OnElementsChanged();
    }

    public void DragBegin()
    {
        _dragContext = OperationService.CreateEditContext(Sheet, Elements);
    }

    public void DragEnd()
    {
        _dragContext?.Dispose();
    }

    protected override void OnElementsChanged()
    {
        _opacity = Mode(e => e.Opacity);
        OnPropertyChanged(nameof(Opacity));
    }
}
