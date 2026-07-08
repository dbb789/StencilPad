using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Widgets;

public class InlineTextField : UserControl
{
    private readonly TextBox _textBox;

    public string Text
    {
        get => _textBox.Text;
        set => _textBox.Text = value;
    }

    public double TextFontSize
    {
        get => _textBox.FontSize;
        set => _textBox.FontSize = value;
    }

    public FontFamily TextFontFamily
    {
        get => _textBox.FontFamily;
        set => _textBox.FontFamily = value;
    }

    public double Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;

            RenderTransform = new TransformGroup
            {
                Children = new TransformCollection
                {
                    new TranslateTransform(-5, -3),
                    new RotateTransform(_rotation)
                }
            };
        }
    }

    private double _rotation;

    public Unit2D TextSize => MeasureText();
    
    public event Action? Committed;
    public event Action? Cancelled;

    public InlineTextField()
    {
        _textBox = new TextBox
        {
            Background = Brushes.White,
            BorderBrush = Brushes.CornflowerBlue,
            Padding = new Thickness(2),
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap
        };

        _textBox.PreviewKeyDown += OnTextBoxKeyDown;
        _textBox.LostFocus += OnTextBoxLostFocus;

        Content = _textBox;

        Loaded += (s, e) => _textBox.Focus();
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                var pos = _textBox.CaretIndex;
                
                _textBox.Text = _textBox.Text.Insert(pos, Environment.NewLine);
                _textBox.CaretIndex = pos + Environment.NewLine.Length;
                
                e.Handled = true;
                return;
            }
            
            Committed?.Invoke();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Cancelled?.Invoke();
            e.Handled = true;
        }
    }

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
        Committed?.Invoke();
    }

    public new void Focus()
    {
        _textBox.Focus();
    }
    
    private Unit2D MeasureText()
    {
        if (string.IsNullOrEmpty(Text))
        {
            return Unit2D.Zero;
        }

        var fontFamily = TextFontFamily;

        var ft = new FormattedText(
            Text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            Unit.FromFontSizePoints(FontSize).Millimeters,
            Brushes.Black,
            1.0);

        return Unit2D.FromMillimeters(ft.Width, ft.Height);
    }
}
