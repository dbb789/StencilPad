using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Rendering;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Export;

public class PngExporter
{
    private const double Dpi = 960.0;
    private const double BaseDpi = 96.0;

    private readonly ILoggerFactory _loggerFactory;
    
    public PngExporter(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }
    
    public void Export(Sheet sheet,
                       string path,
                       ISettings settings,
                       IResourceService resourceService)
    {
        UnitBounds? sheetBounds = null;

        using var resolver = new SheetResolver(_loggerFactory.CreateLogger<SheetResolver>(),
                                               sheet,
                                               settings,
                                               resourceService);

        foreach (var elementResolver in resolver.Elements)
        {
            sheetBounds = UnitBounds.Union(sheetBounds, elementResolver.GetOutlineBounds());
        }

        var bounds = sheetBounds ??
            UnitBounds.FromCenterSize(Unit2D.Zero,
                                      new Unit2D(Unit.FromMillimeters(100),
                                                 Unit.FromMillimeters(100)));
        
        var size = bounds.Size;
        double width  = size.X.Millimeters;
        double height = size.Y.Millimeters;

        using var renderer = new SheetRenderer(_loggerFactory.CreateLogger<SheetRenderer>(),
                                               resolver,
                                               settings,
                                               resourceService);
        
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

        double scale = Dpi / BaseDpi;
        int widthPx  = (int)Math.Round(width * scale);
        int heightPx = (int)Math.Round(height * scale);

        var bitmap = new RenderTargetBitmap(widthPx,
                                            heightPx,
                                            BaseDpi * scale,
                                            BaseDpi * scale,
                                            PixelFormats.Pbgra32);

        bitmap.Render(visual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var stream = File.OpenWrite(path);
        encoder.Save(stream);
    }
}
