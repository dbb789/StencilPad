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

            using var context = _operationService.TryCreateEditContext(_sheet, Elements);

            foreach (var element in Elements)
            {
                element.Opacity = value;
            }

            OnPropertyChanged();
        }
    }

    private readonly Sheet _sheet;
    private readonly IOperationService _operationService;
    private IDisposable? _dragContext;

    public ImagePropertiesViewModel(Sheet sheet,
                                    ISettings settings,
                                    IOperationService operationService)
        : base(sheet, settings)
    {
        _sheet = sheet;
        _operationService = operationService;

        OnElementsChanged();
    }

    public void DragBegin()
    {
        _dragContext = _operationService.CreateEditContext(_sheet, Elements);
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
