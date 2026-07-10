using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class ProjectSchema
{
    public int Version { get; set; }

    public UnitSystem UnitSystem { get; set; } = UnitSystem.Metric;
    public Fraction UnitRatio { get; set; } = Fraction.One;
    public Unit GridSpacingMetric { get; set; } = Unit.FromMillimeters(10);
    public int GridSubdivisionsMetric { get; set; } = 5;
    public Unit GridSpacingImperial { get; set; } = Unit.FromInches(0.25);
    public int GridSubdivisionsImperial { get; set; } = 4;
    
    public SheetSchema[] Sheets { get; set; } = [];
    public SheetElementSchema[] Defaults { get; set; } = [];

    public static ProjectSchema Pack(Project project, int version)
    {
        return new ProjectSchema
        {
            Version = version,
            UnitSystem = project.UnitSystem,
            UnitRatio = project.UnitRatio,
            GridSpacingMetric = project.GridSpacingMetric,
            GridSubdivisionsMetric = project.GridSubdivisionsMetric,
            GridSpacingImperial = project.GridSpacingImperial,
            GridSubdivisionsImperial = project.GridSubdivisionsImperial,
            Sheets = project.Sheets.Select(SheetSchema.Pack).ToArray(),
            Defaults = project.DefaultElements
                              .Select(SheetElementSchema.Pack)
                              .OfType<SheetElementSchema>()
                              .ToArray()
        };
    }

    public static void Unpack(ProjectSchema data, Project target)
    {
        target.Clear();

        target.UnitSystem = data.UnitSystem;
        target.UnitRatio = data.UnitRatio;
        target.GridSpacingMetric = data.GridSpacingMetric;
        target.GridSubdivisionsMetric = data.GridSubdivisionsMetric;
        target.GridSpacingImperial = data.GridSpacingImperial;
        target.GridSubdivisionsImperial = data.GridSubdivisionsImperial;

        foreach (var defaultData in data.Defaults)
        {
            target.SetElementStyle(defaultData.Unpack());
        }

        foreach (var sheetData in data.Sheets)
        {
            var sheet = SheetSchema.Unpack(sheetData);

            target.Sheets.Add(sheet.Id, sheet);
        }
    }
}
