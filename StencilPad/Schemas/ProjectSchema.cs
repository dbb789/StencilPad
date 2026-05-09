using StencilPad.Models;

namespace StencilPad.Schemas;

public class ProjectSchema
{
    public int Version { get; set; }
    public SheetSchema[] Sheets { get; set; } = [];

    public static ProjectSchema Pack(Project project, int version)
    {
        return new ProjectSchema
        {
            Version = version,
            Sheets = project.Sheets.Select(SheetSchema.Pack).ToArray()
        };
    }

    public static void Unpack(ProjectSchema data, Project target)
    {
        target.Clear();

        foreach (var sheetData in data.Sheets)
        {
            target.AddSheet(SheetSchema.Unpack(sheetData));
        }
    }
}
