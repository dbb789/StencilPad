using System.IO;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Spatial;
using StencilPad.Export;

namespace StencilPad.Services;

public class ImportExportService : IImportExportService
{
    private readonly IDialogService _dialogService;
    private readonly ISettings _settings;
    private readonly IResourceService _resourceService;
    private readonly IOperationService _operationService;

    public ImportExportService(IDialogService dialogService,
                               ISettings settings,
                               IResourceService resourceService,
                               IOperationService operationService)
    {
        _dialogService = dialogService;
        _settings = settings;
        _resourceService = resourceService;
        _operationService = operationService;
    }
    
    public async Task ImportImageAsync(Sheet sheet, IViewport viewport)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Import Image",
            Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        byte[] imageData;
        
        try
        {
            imageData = await File.ReadAllBytesAsync(dialog.FileName);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Could not read image file: {ex.Message}", "Import Failed");
            return;
        }

        var size = MeasureImageSize(imageData);
        var halfSize = size / 2.0;
        var center = Unit2D.Zero;

        var element = new ImageElement(center - halfSize, center + halfSize, imageData);

        _operationService.Push(new AddSheetElementOperation(sheet, element));
    }
    
    public void ExportSvg(Sheet sheet)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Export SVG",
            Filter = "SVG Files (*.svg)|*.svg",
            FileName = $"{sheet.Name}.svg"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            SvgExporter.Export(sheet, _settings, _resourceService, dialog.FileName);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to export SVG: {ex.Message}", "Export Failed");
        }
    }

    public void ExportPng(Sheet sheet)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Export PNG",
            Filter = "PNG Files (*.png)|*.png",
            FileName = $"{sheet.Name}.png"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            PngExporter.Export(sheet, dialog.FileName, _settings, _resourceService);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to export PNG: {ex.Message}", "Export Failed");
        }
    }

    private static Unit2D MeasureImageSize(byte[] imageData, double maxMm = 150.0)
    {
        var bitmap = new BitmapImage();

        bitmap.BeginInit();
        bitmap.StreamSource = new MemoryStream(imageData);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();

        var dpiX = bitmap.DpiX > 0 ? bitmap.DpiX : 96.0;
        var dpiY = bitmap.DpiY > 0 ? bitmap.DpiY : 96.0;

        var widthMm = bitmap.PixelWidth * 25.4 / dpiX;
        var heightMm = bitmap.PixelHeight * 25.4 / dpiY;

        var larger = Math.Max(widthMm, heightMm);

        if (larger > maxMm)
        {
            var scale = maxMm / larger;
            widthMm *= scale;
            heightMm *= scale;
        }

        return new Unit2D(Unit.FromMillimeters(widthMm), Unit.FromMillimeters(heightMm));
    }
}
