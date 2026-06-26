using System.IO;
using Microsoft.Win32;
using StencilPad.Models;
using StencilPad.Schemas;

namespace StencilPad.Services;

public class FileService : IFileService
{
    private const string FileExtension = ".spad";
    private const string FileFilter = "StencilPad Files (*.spad)|*.spad";
    private const int FileVersion = 1;

    public async Task<string?> OpenAsync(Project target)
    {
        var dialog = new OpenFileDialog
        {
            Filter = FileFilter,
            DefaultExt = FileExtension
        };

        if (dialog.ShowDialog() != true)
        {
            return null;
        }

        ProjectSchema schema;

        try
        {
            schema = await SchemaUtil.LoadProjectAsync(dialog.FileName);
        }
        catch (Exception e)
        {
            throw new FileServiceException($"Failed to load file: {e.Message}", e);
        }

        if (schema.Version != FileVersion)
        {
            throw new FileServiceException(
                $"Unsupported file version {schema.Version}. This version of StencilPad supports version {FileVersion}.");
        }

        target.Clear(); // Safety
        ProjectSchema.Unpack(schema, target);

        return dialog.FileName;
    }

    public async Task SaveAsync(Project project, string filePath)
    {
        try
        {
            // Write to a temporary file first to avoid data loss in case of an error during the write process.
            var tempFilePath = filePath + ".tmp." + Guid.NewGuid().ToString("N");
            
            await SchemaUtil.SaveProjectAsync(ProjectSchema.Pack(project, FileVersion), tempFilePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.Move(tempFilePath, filePath);
        }
        catch (Exception e)
        {
            throw new FileServiceException($"Failed to write file: {e.Message}", e);
        }
    }

    public async Task<string?> SaveAsAsync(Project project)
    {
        var dialog = new SaveFileDialog
        {
            Filter = FileFilter,
            DefaultExt = FileExtension
        };

        if (dialog.ShowDialog() != true)
        {
            return null;
        }

        await SaveAsync(project, dialog.FileName);
        
        return dialog.FileName;
    }
}
