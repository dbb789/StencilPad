using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using StencilPad.Models;
using StencilPad.Schemas;

namespace StencilPad.Services;

public class FileService : IFileService
{
    private const string FileExtension = ".lcad";
    private const string FileFilter = "StencilPad Files (*.lcad)|*.lcad";
    private const int FileVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = SchemaJsonOptions.Default;

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

        string json;

        try
        {
            json = await File.ReadAllTextAsync(dialog.FileName);
        }
        catch (Exception ex)
        {
            throw new FileServiceException($"Failed to read file: {ex.Message}", ex);
        }

        ProjectSchema schema;

        try
        {
            schema = JsonSerializer.Deserialize<ProjectSchema>(json, JsonOptions)
                ?? throw new FileServiceException("File is empty or could not be parsed.");
        }
        catch (JsonException ex)
        {
            throw new FileServiceException($"File is not a valid StencilPad file: {ex.Message}", ex);
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
        string json;

        try
        {
            json = JsonSerializer.Serialize(ProjectSchema.Pack(project, FileVersion), JsonOptions);
        }
        catch (Exception ex)
        {
            throw new FileServiceException($"Failed to serialise project: {ex.Message}", ex);
        }

        try
        {
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            throw new FileServiceException($"Failed to write file: {ex.Message}", ex);
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
