using System.Diagnostics;
using System.IO;
using System.Text.Json;
using StencilPad.Models;
using StencilPad.Schemas;
using StencilPad.Spatial;

namespace StencilPad.Services;

public static class GeometryResourceLibrary
{
    private static readonly string ResourcesDirectory = "Resources";
    private static readonly string GeometryDirectory = Path.Combine(ResourcesDirectory, "Geometry");
    private static readonly string LibraryIndex = Path.Combine(GeometryDirectory, "Index.json");
    
    public record Entry
    {
        public static Entry Cap(GeometryResourceId id, string filename, Unit2D? size = null)
        {
            return new Entry(GeometryResourceType.Cap, id, filename, size);
        }
        
        public static Entry Marker(GeometryResourceId id, string filename, Unit2D? size = null)
        {
            return new Entry(GeometryResourceType.Marker, id, filename, size);
        }

        public GeometryResourceType Type { get; init; }
        public GeometryResourceId Id { get; init; }
        public string Filename { get; init; }
        public Unit2D? Size { get; init; }

        public Entry(GeometryResourceType type,
                     GeometryResourceId id,
                     string filename,
                     Unit2D? size = null)
        {
            Type = type;
            Id = id;
            Filename = Path.Combine(GeometryDirectory, filename);
            Size = size;
        }
    }
    
    public static IEnumerable<Entry> Load()
    {
        if (!File.Exists(LibraryIndex))
        {
            Debug.WriteLine($"Geometry resource library index file not found at '{LibraryIndex}'.");
            return [];
        }

        var list = new List<Entry>();
        
        try
        {
            string json = File.ReadAllText(LibraryIndex);
            var library = JsonSerializer.Deserialize<GeometryResourceLibrarySchema>(json, SchemaJsonOptions.Default);
            
            foreach (var entry in library?.Caps ?? [])
            {
                list.Add(Entry.Cap(GeometryResourceId.FromValue(entry.Id), entry.Filename, entry.Size));
            }

            foreach (var entry in library?.Markers ?? [])
            {
                list.Add(Entry.Marker(GeometryResourceId.FromValue(entry.Id), entry.Filename, entry.Size));
            }

        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error loading geometry resource library index: {e.Message}");
        }

        return list;
    }
}
