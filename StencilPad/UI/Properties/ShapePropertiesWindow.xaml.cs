using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.UI.Widgets;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class ShapePropertiesWindow : Window
{
    public ShapePropertiesViewModel ViewModel { get; }

    public ShapePropertiesWindow(Sheet sheet,
                                 IResourceService resourceService,
                                 IOperationService operationService)
    {
        InitializeComponent();

        ViewModel = new ShapePropertiesViewModel(sheet, resourceService, operationService);
        DataContext = ViewModel;
        
        var startCapItems = ViewModel.CapIds.Select(
            id => new GeometryDropdown.Entry(
                CreateCapGeometry(resourceService, id, true))).ToList();

        StartCapDropdown.Items = startCapItems;

        var endCapItems = ViewModel.CapIds.Select(
            id => new GeometryDropdown.Entry(
                CreateCapGeometry(resourceService, id, false))).ToList();

        EndCapDropdown.Items = endCapItems;

        var lineStyleItems = ViewModel.LineStyleIds.Select(
            id => new GeometryDropdown.Entry(CreateLineStyleGeometry(),
                                             resourceService.Get(id))).ToList();

        LineStyleDropdown.Items = lineStyleItems;

        Loaded += (_, _) =>
        {
            FillColorField.DragBegin += ViewModel.DragBegin;
            FillColorField.DragEnd += ViewModel.DragEnd;
            LineColorField.DragBegin += ViewModel.DragBegin;
            LineColorField.DragEnd += ViewModel.DragEnd;
        };

        Unloaded += (_, _) =>
        {
            FillColorField.DragBegin -= ViewModel.DragBegin;
            FillColorField.DragEnd -= ViewModel.DragEnd;
            LineColorField.DragBegin -= ViewModel.DragBegin;
            LineColorField.DragEnd -= ViewModel.DragEnd;
        };
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        ViewModel.Dispose();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private Geometry CreateCapGeometry(IResourceService resourceService,
                                       GeometryResourceId resourceId,
                                       bool startCap)
    {
        var cap = resourceService.Get(resourceId);
        
        var line = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        var offset = cap.Size.Y.Millimeters;

        using (var ctx = line.Open())
        {
            ctx.BeginFigure(new Point(0, offset), true, false);
            ctx.LineTo(new Point(0, 4), true, false);
        }

        line.Freeze();

        var group = new GeometryGroup
        {
            FillRule = FillRule.EvenOdd
        };
        
        group.Children.Add(cap.Geometry);
        group.Children.Add(line);

        var transformGroup = new TransformGroup();
        
        if (startCap)
        {
            transformGroup.Children.Add(new RotateTransform(-90, 0, 0));
            transformGroup.Children.Add(new TranslateTransform(0, cap.Size.X.Millimeters / 2));
        }
        else
        {
            transformGroup.Children.Add(new RotateTransform(90, 0, 0));
            transformGroup.Children.Add(new TranslateTransform(4, cap.Size.X.Millimeters / 2));
        }

        transformGroup.Children.Add(new ScaleTransform(4, 4));

        transformGroup.Freeze();
        
        group.Transform = transformGroup;
        group.Freeze();
        
        return group;
    }

    private Geometry CreateLineStyleGeometry()
    {
        var line = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = line.Open())
        {
            ctx.BeginFigure(new Point(0, 0), true, false);
            ctx.LineTo(new Point(80, 0), true, false);
        }

        line.Freeze();

        return line;
    }
}
