using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Rendering;

namespace StencilPad.Services;

public class PrintService : IPrintService
{
    private readonly ILogger<PrintService> _logger;
    private readonly SheetResolver.Factory _sheetResolverFactory;
    private readonly SheetRenderer.Factory _sheetRendererFactory;

    public PrintService(ILogger<PrintService> logger,
                        SheetResolver.Factory sheetResolverFactory,
                        SheetRenderer.Factory sheetRendererFactory)
    {
        _logger = logger;
        _sheetResolverFactory = sheetResolverFactory;
        _sheetRendererFactory = sheetRendererFactory;

    }
    
    public Task<bool> PrintAsync(string documentName, Sheet sheet)
    {
        return PrintAsync(documentName, sheet.Format, (dc) =>
        {
            using var resolver = _sheetResolverFactory.Create(sheet);
            using var renderer = _sheetRendererFactory.Create(resolver);

            dc.PushTransform(new ScaleTransform(1, -1));

            renderer.Render(dc);
            
            dc.Pop();
        });
    }
    
    private async Task<bool> PrintAsync(string documentName,
                                        SheetFormat format,
                                        Action<DrawingContext> drawFunc)
    {
        try
        {
            return await Application.Current.Dispatcher.InvokeAsync(() => Print(documentName, format, drawFunc));
        }
        catch (Exception e)
        {
            _logger.LogError("Print error: {Message}", e.Message);
            return false;
        }
    }

    private bool Print(string documentName, SheetFormat format, Action<DrawingContext> drawFunc)
    {
        try
        {
            var printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                var sheetIsLandscape = format.Orientation == SheetOrientation.Landscape;
                var pageIsLandscape = printDialog.PrintableAreaWidth > printDialog.PrintableAreaHeight;
                var rotate = sheetIsLandscape != pageIsLandscape;

                var visual = BuildVisual(drawFunc, printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight, rotate);

                _logger.LogInformation("Printing {DocumentName} to {PrintQueue}", documentName, printDialog.PrintQueue.FullName);
                
                printDialog.PrintVisual(visual, documentName);
                
                _logger.LogInformation("Print job sent: {DocumentName}", documentName);
                return true;
            }

            _logger.LogInformation("Print cancelled by user");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("Print failed: {Message}", e.Message);
            
            return false;
        }
    }

    private DrawingVisual BuildVisual(Action<DrawingContext> drawFunc, double pageWidth, double pageHeight, bool rotate)
    {
        // Geometry is in mm centred at (0,0). WPF print units are DIPs (1/96 inch).
        // Scale mm => DIPs then translate so (0,0) maps to the page centre.
        const double mmToDip = 96.0 / 25.4;

        var transform = new TransformGroup();
        
        transform.Children.Add(new ScaleTransform(mmToDip, mmToDip));

        if (rotate)
        {
            transform.Children.Add(new RotateTransform(90));
        }
        
        transform.Children.Add(new TranslateTransform(pageWidth / 2, pageHeight / 2));

        var visual = new DrawingVisual();

        using (var dc = visual.RenderOpen())
        {
            dc.PushTransform(transform);

            drawFunc?.Invoke(dc);

            dc.Pop();
        }

        return visual;
    }
}
