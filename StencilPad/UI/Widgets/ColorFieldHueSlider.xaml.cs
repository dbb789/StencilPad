using System.Windows.Controls;

namespace StencilPad.UI.Widgets;

public partial class ColorFieldHueSlider : ColorFieldSliderBase
{
    protected override double DisplayScale => 360;

    public ColorFieldHueSlider()
    {
        InitializeComponent();
        InitializeSlider(DragCanvas, Marker);
    }

    protected override void UpdateGradient() { }
}
