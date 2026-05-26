using System.Globalization;
using System.Windows.Media;

namespace StencilPad.Common;

public static class ColorUtil
{
    public static string ToHexString(Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public static bool TryParseHex(string text, out Color color)
    {
        color = Colors.Black;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }
        
        var s = text.TrimStart('#');

        if (s.Length == 6 &&
            byte.TryParse(s[0..2], NumberStyles.HexNumber, null, out var r) &&
            byte.TryParse(s[2..4], NumberStyles.HexNumber, null, out var g) &&
            byte.TryParse(s[4..6], NumberStyles.HexNumber, null, out var b))
        {
            color = Color.FromArgb(255, r, g, b);
            return true;
        }
        
        if (s.Length == 8 &&
            byte.TryParse(s[0..2], NumberStyles.HexNumber, null, out var a2) &&
            byte.TryParse(s[2..4], NumberStyles.HexNumber, null, out var r2) &&
            byte.TryParse(s[4..6], NumberStyles.HexNumber, null, out var g2) &&
            byte.TryParse(s[6..8], NumberStyles.HexNumber, null, out var b2))
        {
            color = Color.FromArgb(a2, r2, g2, b2);
            return true;
        }

        return false;
    }

    public static void RgbToHsv(Color color, out double h, out double s, out double v)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        v = max;
        s = max == 0 ? 0 : delta / max;

        if (delta == 0)
        {
            h = 0;
            return;
        }

        if (max == r)
        {
            h = 60 * (((g - b) / delta) % 6);
        }
        else if (max == g)
        {
            h = 60 * (((b - r) / delta) + 2);
        }
        else
        {
            h = 60 * (((r - g) / delta) + 4);
        }

        if (h < 0)
        {
            h += 360;
        }
    }

    public static Color HsvToRgb(double h, double s, double v, double a)
    {
        if (s == 0)
        {
            var grey = (byte)(v * 255);
            
            return Color.FromArgb((byte)(a * 255), grey, grey, grey);
        }

        h /= 60;

        var i = (int)Math.Floor(h);
        var f = h - i;
        var p = v * (1 - s);
        var q = v * (1 - s * f);
        var t = v * (1 - s * (1 - f));

        double r;
        double g;
        double b;

        switch (i % 6)
        {
        case 0:  r = v; g = t; b = p; break;
        case 1:  r = q; g = v; b = p; break;
        case 2:  r = p; g = v; b = t; break;
        case 3:  r = p; g = q; b = v; break;
        case 4:  r = t; g = p; b = v; break;
        default: r = v; g = p; b = q; break;
        }

        return Color.FromArgb((byte)(a * 255), (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    public static Color WithAlpha(Color color, byte alpha)
    {
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }
}
