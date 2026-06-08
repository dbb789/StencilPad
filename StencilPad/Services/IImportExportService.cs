using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Services;

public interface IImportExportService
{
    Task ImportImageAsync(Sheet sheet, IViewport viewport);
    void ExportSvg(Sheet sheet);
    void ExportPng(Sheet sheet);
}
