using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StencilPad.Models;
using StencilPad.Rendering;
using StencilPad.Services;

namespace StencilPad.Export;

public static class PngExporter
{
    private const double Dpi = 96.0;
    private const double MmPerInch = 25.4;
    private const double PixelsPerMm = Dpi / MmPerInch;

    public static void Export(Sheet sheet, string path, IResourceService resourceService)
    {
        var size = sheet.Format.Size;
        double widthMm  = size.X.Millimeters;
        double heightMm = size.Y.Millimeters;
        int widthPx  = (int)Math.Round(widthMm  * PixelsPerMm);
        int heightPx = (int)Math.Round(heightMm * PixelsPerMm);

        var factory  = new SheetElementRendererFactory(resourceService);
        var renderer = new SheetRenderer(factory);
        renderer.Sheet = sheet;

        var transform = new TransformGroup();
        transform.Children.Add(new TranslateTransform(widthMm / 2.0, heightMm / 2.0));
        transform.Children.Add(new ScaleTransform(PixelsPerMm, PixelsPerMm));
        transform.Freeze();

        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, widthPx, heightPx));
            dc.PushTransform(transform);
            renderer.Render(dc);
            dc.Pop();
        }

        renderer.Dispose();

        var bitmap = new RenderTargetBitmap(widthPx, heightPx, Dpi, Dpi, PixelFormats.Pbgra32);
        bitmap.Render(visual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var stream = File.OpenWrite(path);
        encoder.Save(stream);
    }
}
