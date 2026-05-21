using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class ImageElementEditRenderer : SheetElementEditRenderer
{
    private static Pen OutlinePen;

    static ImageElementEditRenderer()
    {
        OutlinePen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), 0.2)
        {
            DashStyle = DashStyles.Dot
        };
        
        OutlinePen.Freeze();
    }
    
    private readonly ImageElement _imageElement;

    public ImageElementEditRenderer(ImageElement imageElement)
    {
        _imageElement = imageElement;
        _imageElement.GeometryChanged += OnGeometryChanged;
        _imageElement.PropertyChanged += OnPropertyChanged;
    }

    public override void Dispose()
    {
        _imageElement.GeometryChanged -= OnGeometryChanged;
        _imageElement.PropertyChanged -= OnPropertyChanged;
    }

    public override void Render(DrawingContext dc)
    {
        var bounds = UnitBounds.FromMinMax(_imageElement.Min, _imageElement.Max);
        
        if (bounds.Size == Unit2D.Zero)
        {
            return;
        }

        var transform = CreateTransform();
        dc.PushTransform(transform);
        dc.DrawRectangle(Brushes.Transparent, OutlinePen, bounds.Millimeters);
        dc.Pop();
    }

    private Transform CreateTransform()
    {
        var group = new TransformGroup();
        if (_imageElement.Transform.Angle != 0m)
        {
            group.Children.Add(new RotateTransform((double)_imageElement.Transform.Angle));
        }
        group.Children.Add(new TranslateTransform(_imageElement.Transform.Position.X.Millimeters,
                                                  _imageElement.Transform.Position.Y.Millimeters));
        group.Freeze();
        return group;
    }

    private void OnGeometryChanged(ISheetElement _) => InvokeRendererDirty();

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImageElement.Transform))
        {
            InvokeRendererDirty();
        }
    }
}
