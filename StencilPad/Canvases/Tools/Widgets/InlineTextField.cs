using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

    public event Action? Committed;
    public event Action? Cancelled;

    public InlineTextField()
    {
        _textBox = new TextBox
        {
            Background = Brushes.White,
            BorderBrush = Brushes.CornflowerBlue,
            MinWidth = 100,
            Padding = new Thickness(0),
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
        };

        _textBox.KeyDown += OnTextBoxKeyDown;
        _textBox.LostFocus += OnTextBoxLostFocus;

        Content = _textBox;

        Loaded += (s, e) => _textBox.Focus();
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
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
}
