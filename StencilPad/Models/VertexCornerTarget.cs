namespace StencilPad.Models;

public readonly record struct VertexCornerTarget(ISheetElement Element, EditablePolygon Polygon, int VertexIndex);
