using System.IO;
using System.Text.Json;

namespace StencilPad.Schemas;

public static class SchemaUtil
{
    public static ProjectSchema LoadProject(string filename)
    {
        var json = File.ReadAllText(filename);
        
        return DeserializeProject(json);
    }
    
    public static async Task<ProjectSchema> LoadProjectAsync(string filename)
    {
        var json = await File.ReadAllTextAsync(filename);

        return DeserializeProject(json);
    }

    private static ProjectSchema DeserializeProject(string json)
    {
        var schema = JsonSerializer.Deserialize<ProjectSchema>(json, SchemaJsonOptions.Default);

        if (schema == null)
        {
            throw new InvalidDataException("Failed to deserialize project schema.");
        }

        return schema;
    }
    
    public static async Task SaveProjectAsync(ProjectSchema schema, string filename)
    {
        var json = JsonSerializer.Serialize(schema, SchemaJsonOptions.Default);
        
        await File.WriteAllTextAsync(filename, json);
    }
}
