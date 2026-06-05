using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.UI.Widgets;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class MarkerPathPropertiesWindow : Window
{
    public MarkerPathPropertiesViewModel ViewModel { get; }

    public MarkerPathPropertiesWindow(Sheet sheet,
                                      IResourceService resourceService,
                                      IOperationService operationService)
    {
        InitializeComponent();
        
        ViewModel = new MarkerPathPropertiesViewModel(sheet,
                                                      resourceService,
                                                      operationService);
        DataContext = ViewModel;

        var markerTypeItems = ViewModel.MarkerTypeIds.Select(
            id => new GeometryDropdown.Entry(
                CreateMarkerGeometry(resourceService, id))).ToList();

        MarkerTypeDropdown.Items = markerTypeItems;

        Loaded += (_, _) =>
        {
            MarkerColorField.DragBegin += ViewModel.DragBegin;
            MarkerColorField.DragEnd += ViewModel.DragEnd;
            LineColorField.DragBegin += ViewModel.DragBegin;
            LineColorField.DragEnd += ViewModel.DragEnd;
        };

        Unloaded += (_, _) =>
        {
            MarkerColorField.DragBegin -= ViewModel.DragBegin;
            MarkerColorField.DragEnd -= ViewModel.DragEnd;
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

    private Geometry CreateMarkerGeometry(IResourceService resourceService,
                                          GeometryResourceId resourceId)
    {
        var group = new GeometryGroup
        {
            FillRule = FillRule.EvenOdd
        };

        var marker = resourceService.Get(resourceId);
        
        group.Children.Add(marker.Geometry);

        var transformGroup = new TransformGroup();

        transformGroup.Children.Add(new TranslateTransform(4, marker.Size.X.Millimeters / 2));
        transformGroup.Children.Add(new ScaleTransform(5, 5));

        transformGroup.Freeze();

        group.Transform = transformGroup;
        group.Freeze();

        return group;
    }
}
