using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Rendering;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class EditTool : ITool
{
    public class Factory(Sheet Sheet,
                         IHandleMap HandleMap,
                         ToolOverlay ToolOverlay,
                         IEditOverlayRenderer EditOverlayRenderer,
                         IRubberBand RubberBand,
                         IOperationService OperationService,
                         Factory<EditToolOverlay> EditToolOverlayFactory) : IToolFactory
    {
        public string IconResource => "EditTool";
        public string Tooltip => "Edit Points";

        public ITool Create(IToolButton button)
        {
            return new EditTool(button,
                                Sheet,
                                HandleMap,
                                ToolOverlay,
                                EditOverlayRenderer,
                                RubberBand,
                                OperationService,
                                EditToolOverlayFactory);
        }
    }

    private readonly IToolButton _button;
    private readonly Sheet _sheet;
    private readonly IHandleMap _handleMap;
    private readonly ToolOverlay _toolOverlay;
    private readonly IEditOverlayRenderer _editOverlayRenderer;
    private readonly IRubberBand _rubberBand;
    private readonly IOperationService _operationService;
    private readonly Factory<EditToolOverlay> _overlayFactory;
    
    private readonly List<ISheetElement> _selection;
    private readonly List<Unit2D> _originalPositions;
    
    private EditToolOverlay? _overlay;
    private EditSheetElementContext? _editContext;

    private EditTool(IToolButton button,
                     Sheet sheet,
                     IHandleMap handleMap,
                     ToolOverlay toolOverlay,
                     IEditOverlayRenderer editOverlayRenderer,
                     IRubberBand rubberBand,
                     IOperationService operationService,
                     Factory<EditToolOverlay> overlayFactory)
    {
        _button = button;
        _sheet = sheet;
        _handleMap = handleMap;
        _toolOverlay = toolOverlay;
        _editOverlayRenderer = editOverlayRenderer;
        _rubberBand = rubberBand;
        _operationService = operationService;
        _overlayFactory = overlayFactory;
        
        _editContext = null;
        _selection = new(GetEditableSelection());
        _originalPositions = new(64);
        _button.IsEnabled = _selection.Count > 0;
        _sheet.Selection.CollectionChanged += OnSelectionChanged;
    }

    public void Dispose()
    {
        _sheet.Selection.CollectionChanged -= OnSelectionChanged;
    }

    public void ToolBegin()
    {
        if (_selection.Count == 0)
        {
            return;
        }
        
        _overlay = _overlayFactory.Create();
        
        _toolOverlay.ActiveOverlay = _overlay;
        _editOverlayRenderer.IsEnabled = true;

        _overlay.HandleDragBegin += OnHandleDragBegin;
        _overlay.HandleDragged += OnHandleDragged;
        _overlay.HandleDragEnd += OnHandleDragEnd;
        _overlay.HandleSelected += OnHandleSelected;
        _overlay.ActionInvoked += ActionInvoked;
        
        _rubberBand.BoundsSelected += OnBoundsSelected;
        _rubberBand.PointSelected += OnPointSelected;
    }

    public void ToolEnd()
    {
        _toolOverlay.ActiveOverlay = null;
        _editOverlayRenderer.IsEnabled = false;

        if (_overlay is not null)
        {
            _overlay.HandleDragBegin -= OnHandleDragBegin;
            _overlay.HandleDragged -= OnHandleDragged;
            _overlay.HandleDragEnd -= OnHandleDragEnd;
            _overlay.HandleSelected -= OnHandleSelected;
            _overlay.ActionInvoked -= ActionInvoked;
            _overlay.Dispose();
            _overlay = null;
        }

        _rubberBand.BoundsSelected -= OnBoundsSelected;
        _rubberBand.PointSelected -= OnPointSelected;
    }

    private void OnHandleDragBegin(ISheetElement element,
                                   Handle handle)
    {
        if (_handleMap.TryGetHandleEntry(handle, out var entry))
        {
            if (!entry.Selected)
            {
                _handleMap.ClearSelection();
                entry.SetSelected(true);
            }
        }
        
        _editContext = new EditSheetElementContext(_sheet, _selection);
    }

    private void OnHandleDragged(ISheetElement element,
                                 Handle handle,
                                 Unit2D delta)
    {
        if (!handle.CanGroupMove)
        {
            if (_handleMap.TryGetHandleEntry(handle, out var entry))
            {
                entry.SetPosition(entry.Position + delta);
            }
            return;
        }

        // Sometimes, say in the case of a bounds handle, multiple handles that
        // can affect each other are dragged at once. So we need to store their
        // original positions, and apply the delta to those, instead of applying
        // the delta to the current position, which may have already been
        // modified by another handle. And (hopefully) they won't fight each
        // other.
        
        var selectedHandles = _handleMap.SelectedHandles;
        
        _originalPositions.Clear();
        
        for (int i = 0; i < selectedHandles.Count; ++i)
        {
            var entry = selectedHandles[i];

            if (entry.Handle.CanGroupMove)
            {
                _originalPositions.Add(entry.Position);
            }
        }

        for (int i = 0; i < selectedHandles.Count; ++i)
        {
            var entry = selectedHandles[i];

            if (entry.Handle.CanGroupMove)
            {
                entry.SetPosition(_originalPositions[i] + delta);
            }
        }
    }

    private void OnHandleDragEnd()
    {
        if (_editContext is null)
        {
            return;
        }
        
        _operationService.Push(_editContext.FlushOperation());
        _editContext = null;
    }
    
    private void OnBoundsSelected(UnitBounds bounds)
    {
        if (_selection.Count == 0)
        {
            return;
        }

        var modifyingSelection = ModifierUtil.IsModifyingSelection();

        var selected = new List<IHandleMapEntry>();
        
        _handleMap.QueryHandles(bounds, selected);

        if (!modifyingSelection)
        {
            _handleMap.ClearSelection();
        }
        
        foreach (var entry in selected)
        {
            if (entry.Editing)
            {
                entry.SetSelected(true);
            }
        }
    }

    private void OnPointSelected(Unit2D point)
    {
        if (_selection.Count == 0)
        {
            return;
        }

        _handleMap.ClearSelection();
    }

    private void OnHandleSelected(ISheetElement element,
                                  Handle handle)
    {
        var modifyingSelection = ModifierUtil.IsModifyingSelection();

        if (modifyingSelection)
        {
            if (_handleMap.TryGetHandleEntry(handle, out var entry))
            {
                entry.SetSelected(!entry.Selected);
            }
        }
        else
        {
            _handleMap.ClearSelection();

            if (_handleMap.TryGetHandleEntry(handle, out var entry))
            {
                entry.SetSelected(true);
            }
        }
    }
    
    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        _selection.Clear();
        _selection.AddRange(GetEditableSelection());
        
        _button.IsEnabled = _selection.Count > 0;
    }

    private void ActionInvoked(ISheetElementAction action)
    {
        var editContext = new EditSheetElementContext(_sheet, _selection);
        
        action.Invoke(_sheet, _selection);
        
        _operationService.Push(editContext.FlushOperation());
    }
    
    private IEnumerable<ISheetElement> GetEditableSelection()
    {
        return _sheet.Selection;
    }
}

