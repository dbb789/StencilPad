using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Rendering;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.UI
{
    public partial class SheetCanvas : UserControl, IToolContext
    {
        public static readonly DependencyProperty SheetProperty =
            DependencyProperty.Register(nameof(Sheet), typeof(Sheet), typeof(SheetCanvas),
                new FrameworkPropertyMetadata(null, OnSheetChanged));

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(SheetCanvas),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnZoomChanged));

        public static readonly DependencyProperty ShowGridProperty =
            DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(SheetCanvas),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnShowGridChanged));

        public static readonly DependencyProperty SnapToGridProperty =
            DependencyProperty.Register(nameof(SnapToGrid), typeof(bool), typeof(SheetCanvas),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSnapChanged));
        
        public static readonly DependencyProperty SnapToPointProperty =
            DependencyProperty.Register(nameof(SnapToPoint), typeof(bool), typeof(SheetCanvas),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSnapChanged));

        public Sheet Sheet
        {
            get => (Sheet)GetValue(SheetProperty)!;
            set => SetValue(SheetProperty, value);
        }

        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public bool ShowGrid
        {
            get => (bool)GetValue(ShowGridProperty);
            set => SetValue(ShowGridProperty, value);
        }

        public bool SnapToGrid
        {
            get => (bool)GetValue(SnapToGridProperty);
            set => SetValue(SnapToGridProperty, value);
        }
        
        public bool SnapToPoint
        {
            get => (bool)GetValue(SnapToPointProperty);
            set => SetValue(SnapToPointProperty, value);
        }

        public IViewport Viewport => _viewport;
        public IRubberBand RubberBand => _rubberBandEventPanel;
        public IHandleMap HandleMap => _handleMap;
        public SheetRenderer SheetRenderer => _sheetRenderer;
        public IEditOverlayRenderer EditOverlayRenderer => _editOverlayRenderer;
        public CanvasGrid CanvasGrid => _canvasGrid;
        public SheetRenderPanel Renderer => _renderer;
        public ToolOverlay ToolOverlay => _toolOverlay;
        public IUnitSnap UnitSnap => _unitSnap;
        public UnitSnapOverlay UnitSnapOverlay => _unitSnapOverlay;
        
        private readonly VisualViewport _viewport;
        private readonly HandleMap _handleMap;
        private readonly SheetRenderer _sheetRenderer;
        private readonly EditOverlayRenderer _editOverlayRenderer;
        private readonly CanvasGrid _canvasGrid;
        private readonly RubberBandEventPanel _rubberBandEventPanel;
        private readonly SheetRenderPanel _renderer;
        private readonly ToolOverlay _toolOverlay;
        private readonly RubberBandRenderPanel _rubberBandRenderPanel;
        private readonly UnitSnapOverlay _unitSnapOverlay;
        private readonly CompositeUnitSnap _unitSnap;
        
        public event Action? CanvasReady;
        public event Action? SelectAllRequested;
        public event Action? ClearSelectionRequested;

        public SheetCanvas()
        {   
            _viewport = new VisualViewport();
            _handleMap = new HandleMap();
            _sheetRenderer = new SheetRenderer();
            _editOverlayRenderer = new EditOverlayRenderer();

            _canvasGrid = new CanvasGrid(_viewport);

            _renderer = new SheetRenderPanel(_sheetRenderer,
                                             _editOverlayRenderer,
                                             _viewport);
            _canvasGrid.Content = _renderer;

            _rubberBandEventPanel = new RubberBandEventPanel(_viewport);
            _renderer.Content = _rubberBandEventPanel;

            _unitSnap = new CompositeUnitSnap();
            _unitSnapOverlay = new UnitSnapOverlay(_viewport, _unitSnap);
            _rubberBandEventPanel.Content = _unitSnapOverlay;

            _toolOverlay = new ToolOverlay();
            _unitSnapOverlay.Content = _toolOverlay;

            _rubberBandRenderPanel = new RubberBandRenderPanel();
            _rubberBandEventPanel.Updated += _rubberBandRenderPanel.Updated;
            
            InitializeComponent();

            Focusable = true;
            PreviewMouseDown += (_, _) => Focus();
            CommandBindings.Add(new CommandBinding(
                GlobalCommands.SelectAll,
                (_, _) => SelectAllRequested?.Invoke(),
                (_, e) => e.CanExecute = true));
            CommandBindings.Add(new CommandBinding(
                GlobalCommands.ClearSelection,
                (_, _) => ClearSelectionRequested?.Invoke(),
                (_, e) => e.CanExecute = true));

            _viewport.Visual = this;

            CanvasRoot.Children.Add(_canvasGrid);
            CanvasRoot.Children.Add(_rubberBandRenderPanel);

            _viewport.ViewportChanged += UpdateCanvasSize;
            
            // _unitSnap.Add(_canvasGrid);
            _unitSnap.Add(_handleMap);

            Loaded += (s, e) =>
            {
                UpdateCanvasSize();
                CanvasReady?.Invoke();
            };
        }
        
        private static void OnSheetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SheetCanvas sheetCanvas)
            {
                return;
            }
            
            if (e.OldValue is Sheet oldSheet)
            {
                oldSheet.PropertyChanged -= sheetCanvas.Sheet_PropertyChanged;
            }

            var sheet = e.NewValue as Sheet;

            if (sheet is null)
            {
                return;
            }
            
            sheetCanvas._sheetRenderer.Sheet = sheet;
            sheetCanvas._editOverlayRenderer.Sheet = sheet;
            sheetCanvas._handleMap.Sheet = sheet;
            
            sheet.PropertyChanged += sheetCanvas.Sheet_PropertyChanged;
            sheetCanvas.UpdateViewportSize(sheet.Format);
        }

        private void Sheet_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Sheet.Format) && Sheet is not null)
            {
                UpdateViewportSize(Sheet.Format);
            }
        }

        private void UpdateViewportSize(SheetFormat format)
        {
            var sheetSize = format.Size;
            
            // Calculate 10% of the largest dimension, rounded up to the nearest 10mm
            double maxDim = Math.Max(sheetSize.X.Millimeters, sheetSize.Y.Millimeters);
            double marginMm = Math.Ceiling((maxDim * 0.1) / 10.0) * 10.0;
            var margin = Unit.FromMillimeters(marginMm);
            
            _viewport.SheetSize = sheetSize;
            _viewport.Size = new Unit2D(sheetSize.X + margin * 2,
                                        sheetSize.Y + margin * 2);
        }

        private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SheetCanvas sheetCanvas)
            {
                return;
            }

            sheetCanvas._viewport.Zoom = (double)e.NewValue;
        }

        private static void OnShowGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SheetCanvas sheetCanvas)
            {
                return;
            }

            sheetCanvas._canvasGrid.ShowGrid = (bool)e.NewValue;
        }

        private static void OnSnapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SheetCanvas sheetCanvas)
            {
                return;
            }
            
            sheetCanvas._unitSnap.Clear();

            if (sheetCanvas.SnapToPoint)
            {
                sheetCanvas._unitSnap.Add(sheetCanvas._handleMap);
            }

            if (sheetCanvas.SnapToGrid)
            {
                sheetCanvas._unitSnap.Add(sheetCanvas._canvasGrid);
            }
        }
        
        private void UpdateCanvasSize()
        {
            if (CanvasRoot is null)
            {
                return;
            }
            
            Width = _viewport.ToPixels(_viewport.Size.X);
            Height = _viewport.ToPixels(_viewport.Size.Y);

            _canvasGrid.InvalidateVisual();
            Renderer.InvalidateVisual();
        }
    }
}
