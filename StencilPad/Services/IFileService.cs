using StencilPad.Models;

namespace StencilPad.Services;

public interface IFileService
{
    Task<string?> OpenAsync(Project target);
    Task OpenAsync(string filename, Project target);
    Task SaveAsync(Project project, string filePath);
    Task<string?> SaveAsAsync(Project project, string? filePath = null);
}
