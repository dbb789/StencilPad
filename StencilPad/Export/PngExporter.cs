using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Rendering;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Export;

public static class PngExporter
{
    private const double Dpi = 960.0;
    private const double BaseDpi = 96.0;

    public static void Export(Sheet sheet,
                              string path,
                              ISettings settings,
                              IResourceService resourceService)
    {
        UnitBounds? sheetBounds = null;

        foreach (var element in sheet.Elements)
        {
            sheetBounds = UnitBounds.Union(sheetBounds, element.GetBounds());
        }

        var bounds = sheetBounds ??
            UnitBounds.FromCenterSize(Unit2D.Zero,
                                      new Unit2D(Unit.FromMillimeters(98),
                                                 Unit.FromMillimeters(98)));

        //bounds = bounds.Pad(Unit.FromMillimeters(1));
        
        var size = bounds.Size;
        double width  = size.X.Millimeters;
        double height = size.Y.Millimeters;

        using var resolver = new SheetResolver(sheet, settings, resourceService);
        using var renderer = new SheetRenderer(resolver, settings, resourceService);
        
        var transform = new TransformGroup();
        transform.Children.Add(new ScaleTransform(1, -1));
        transform.Children.Add(new TranslateTransform(-bounds.Min.X.Millimeters,
                                                      -bounds.Min.Y.Millimeters));
        transform.Freeze();

        var visual = new DrawingVisual();

        using (var dc = visual.RenderOpen())
        {
            // dc.DrawRectangle(Brushes.White,
            //                  null,
            //                  new Rect(0, 0, width, height));
            
            dc.PushTransform(transform);
            renderer.Render(dc);
            dc.Pop();
        }

        renderer.Dispose();

        double scale = Dpi / BaseDpi;
        int widthPx  = (int)Math.Round(width * scale);
        int heightPx = (int)Math.Round(height * scale);

        var bitmap = new RenderTargetBitmap(widthPx, heightPx, BaseDpi * scale, BaseDpi * scale, PixelFormats.Pbgra32);

        bitmap.Render(visual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var stream = File.OpenWrite(path);
        encoder.Save(stream);
    }
}
