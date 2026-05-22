using System.Windows.Controls;

namespace StencilPad.UI.Widgets;

public partial class ColorFieldHueSlider : ColorFieldSliderBase
{
    public ColorFieldHueSlider()
    {
        InitializeComponent();
        InitializeSlider(DragCanvas, Marker);
    }

    protected override void UpdateGradient() { }
}
