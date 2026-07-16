namespace StencilPad.Tests.Common;

using System.Windows.Media;
using StencilPad.Common;

public class ColorUtilTests
{
    [Test]
    public void ToHexString_ReturnsCorrectFormat()
    {
        var color = Color.FromArgb(255, 255, 128, 0); // #FFFF8000
        Assert.That(ColorUtil.ToHexString(color), Is.EqualTo("#FF8000FF"));
    }

    [Test]
    public void ToHexStringOpaque_ReturnsCorrectFormat()
    {
        var color = Color.FromArgb(128, 255, 128, 0); // alpha ignored
        Assert.That(ColorUtil.ToHexStringOpaque(color), Is.EqualTo("#FF8000"));
    }

    [TestCase("#FF8000", 255, 255, 128, 0)]
    [TestCase("FF8000", 255, 255, 128, 0)]
    [TestCase("#FFFF8000", 255, 255, 128, 0)]
    [TestCase("#000000", 255, 0, 0, 0)]
    [TestCase("#80112233", 128, 17, 34, 51)]
    public void TryParseHex_ValidHex_ReturnsTrueAndCorrectColor(string hex, int a, int r, int g, int b)
    {
        var result = ColorUtil.TryParseHex(hex, out var color);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(color, Is.EqualTo(Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b)));
        });
    }

    [TestCase("#F80")] // Short format not supported
    [TestCase("#GG0000")] // Invalid chars
    [TestCase("")] // Empty
    public void TryParseHex_InvalidHex_ReturnsFalse(string hex)
    {
        var result = ColorUtil.TryParseHex(hex, out var color);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(color, Is.EqualTo(Colors.Black)); // Default output
        });
    }

    [Test]
    public void RgbToHsv_Black_ReturnsCorrectHsv()
    {
        ColorUtil.RgbToHsv(Colors.Black, out var h, out var s, out var v);
        Assert.Multiple(() =>
        {
            Assert.That(h, Is.EqualTo(0));
            Assert.That(s, Is.EqualTo(0));
            Assert.That(v, Is.EqualTo(0));
        });
    }

    [Test]
    public void RgbToHsv_Red_ReturnsCorrectHsv()
    {
        ColorUtil.RgbToHsv(Colors.Red, out var h, out var s, out var v);
        Assert.Multiple(() =>
        {
            Assert.That(h, Is.EqualTo(0));
            Assert.That(s, Is.EqualTo(1.0));
            Assert.That(v, Is.EqualTo(1.0));
        });
    }

    [Test]
    public void HsvToRgb_Red_ReturnsCorrectRgb()
    {
        var color = ColorUtil.HsvToRgb(0, 1.0, 1.0, 1.0);
        Assert.That(color, Is.EqualTo(Colors.Red));
    }

    [Test]
    public void HsvToRgb_Grey_ReturnsCorrectRgb()
    {
        var color = ColorUtil.HsvToRgb(0, 0, 0.5, 1.0);
        Assert.Multiple(() =>
        {
            Assert.That(color.A, Is.EqualTo(255));
            Assert.That(color.R, Is.EqualTo(127).Within(1)); // 0.5 * 255 = 127.5
            Assert.That(color.G, Is.EqualTo(127).Within(1));
            Assert.That(color.B, Is.EqualTo(127).Within(1));
        });
    }

    [Test]
    public void WithAlpha_ChangesAlphaOnly()
    {
        var color = Colors.Red;
        var transparentRed = ColorUtil.WithAlpha(color, 128);
        Assert.Multiple(() =>
        {
            Assert.That(transparentRed.A, Is.EqualTo(128));
            Assert.That(transparentRed.R, Is.EqualTo(color.R));
            Assert.That(transparentRed.G, Is.EqualTo(color.G));
            Assert.That(transparentRed.B, Is.EqualTo(color.B));
        });
    }
}
