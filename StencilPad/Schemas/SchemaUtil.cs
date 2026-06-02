using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace StencilPad.Schemas;

public static class SchemaUtil
{
    public static ProjectSchema LoadProject(string filename)
    {
        using var file = File.OpenRead(filename);
        using var gz = new GZipStream(file, CompressionMode.Decompress);

        var schema = JsonSerializer.Deserialize<ProjectSchema>(gz, SchemaJsonOptions.Default);

        if (schema is null)
        {
            throw new InvalidDataException("Failed to deserialize project schema.");
        }
        
        return schema;
    }

    public static async Task<ProjectSchema> LoadProjectAsync(string filename)
    {
        await using var file = File.OpenRead(filename);
        await using var gz = new GZipStream(file, CompressionMode.Decompress);

        var schema = await JsonSerializer.DeserializeAsync<ProjectSchema>(gz, SchemaJsonOptions.Default);

        if (schema is null)
        {
            throw new InvalidDataException("Failed to deserialize project schema.");
        }
        
        return schema;
    }

    public static async Task SaveProjectAsync(ProjectSchema schema, string filename)
    {
        await using var file = File.Create(filename);
        await using var gz = new GZipStream(file, CompressionLevel.Optimal);

        await JsonSerializer.SerializeAsync(gz, schema, SchemaJsonOptions.Default);
    }
}
