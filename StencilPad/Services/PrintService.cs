using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Rendering;

namespace StencilPad.Services;

public class PrintService : IPrintService
{
    private readonly IResourceService _resourceService;
    
    public PrintService(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }
    
    public Task<bool> PrintAsync(string documentName, Sheet sheet)
    {
        return PrintAsync(documentName, (dc) =>
        {
            foreach (var element in sheet.Elements)
            {
                var renderer = new SheetElementRenderer(element, _resourceService);
                
                if (renderer is not null)
                {
                    renderer.Render(dc);
                }
            }
        });
    }
    
    private async Task<bool> PrintAsync(string documentName, Action<DrawingContext> drawFunc)
    {
        try
        {
            return await Application.Current.Dispatcher.InvokeAsync(() => Print(documentName, drawFunc));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Print error: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }

    private bool Print(string documentName, Action<DrawingContext> drawFunc)
    {
        try
        {
            var printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                var visual = BuildVisual(drawFunc, printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                
                printDialog.PrintVisual(visual, documentName);
                
                Debug.WriteLine($"Print job sent: {documentName}");
                return true;
            }

            Debug.WriteLine("Print cancelled by user");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Print failed: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }

    private DrawingVisual BuildVisual(Action<DrawingContext> drawFunc, double pageWidth, double pageHeight)
    {
        // Geometry is in mm centred at (0,0). WPF print units are DIPs (1/96 inch).
        // Scale mm => DIPs then translate so (0,0) maps to the page centre.
        const double mmToDip = 96.0 / 25.4;

        var transform = new TransformGroup();
        
        transform.Children.Add(new ScaleTransform(mmToDip, mmToDip));
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
