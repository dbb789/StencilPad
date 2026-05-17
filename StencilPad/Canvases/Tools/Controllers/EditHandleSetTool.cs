using System.Windows.Input;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class EditHandleSetTool : ITool
{
    public class Factory(SheetElementEditActionSet SheetElementEditActions,
                         IOperationService OperationService) : IToolFactory
    {
        public string IconResource => "EditTool";
        public string Tooltip => "Edit Points";

        public ITool Create(IToolButton button, Sheet sheet, IToolContext context)
        {
            return new EditHandleSetTool(button,
                                         sheet,
                                         context,
                                         SheetElementEditActions,
                                         OperationService);
        }
    }

    private readonly IToolButton _button;
    private readonly Sheet _sheet;
    private readonly IToolContext _context;
    private readonly SheetElementEditActionSet _sheetElementEditActions;
    private readonly IOperationService _operationService;
    private readonly List<ISheetElement> _selection;
    
    private EditHandleSetToolOverlay? _overlay;
    private EditSheetElementContext? _editContext;
    
    private EditHandleSetTool(IToolButton button,
                              Sheet sheet,
                              IToolContext context,
                              SheetElementEditActionSet sheetElementEditActions,
                              IOperationService operationService)
    {
        _button = button;
        _sheet = sheet;
        _context = context;
        _sheetElementEditActions = sheetElementEditActions;
        _operationService = operationService;
        _editContext = null;
        _selection = new(GetEditableSelection());
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
        
        _overlay = new EditHandleSetToolOverlay(_context,
                                                _sheet,
                                                _sheetElementEditActions.Actions);
        
        _context.ToolOverlay.ActiveOverlay = _overlay;
        _context.EditOverlayRenderer.IsEnabled = true;

        _overlay.HandleDragBegin += OnHandleDragBegin;
        _overlay.HandleDragged += OnHandleDragged;
        _overlay.HandleDragEnd += OnHandleDragEnd;
        _overlay.HandleSelected += OnHandleSelected;
        _overlay.ActionInvoked += ActionInvoked;
        
        _context.RubberBand.BoundsSelected += OnBoundsSelected;
        _context.RubberBand.PointSelected += OnPointSelected;
    }

    public void ToolEnd()
    {
        _context.ToolOverlay.ActiveOverlay = null;
        _context.EditOverlayRenderer.IsEnabled = false;

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

        _context.RubberBand.BoundsSelected -= OnBoundsSelected;
        _context.RubberBand.PointSelected -= OnPointSelected;
    }

    private void OnHandleDragBegin(IHandleSource source,
                                   Handle handle)
    {
        if (_context.HandleMap.TryGetHandleEntry(handle, out var entry))
        {
            if (!entry.Selected)
            {
                _context.HandleMap.ClearSelection();
                entry.SetSelected(true);
            }
        }
        
        _editContext = new EditSheetElementContext(_sheet, _selection);
    }

    private void OnHandleDragged(IHandleSource source,
                                 Handle handle,
                                 Unit2D delta)
    {
        if (!handle.CanGroupMove)
        {
            source.SetPoint(handle, source.GetPoint(handle) + delta);
            return;
        }

        foreach (var entry in _context.HandleMap.SelectedHandles)
        {
            if (entry.Handle.CanGroupMove)
            {
               entry.SetPosition(entry.Position + delta);
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

        var modifyingSelection = IsModifyingSelection();

        var selected = new List<IHandleMapEntry>();
        
        _context.HandleMap.QueryHandles(bounds, selected);

        if (!modifyingSelection)
        {
            _context.HandleMap.ClearSelection();
        }
        
        foreach (var entry in selected)
        {
            entry.SetSelected(true);
        }
    }

    private void OnPointSelected(Unit2D point)
    {
        if (_selection.Count == 0)
        {
            return;
        }

        _context.HandleMap.ClearSelection();
    }

    private void OnHandleSelected(IHandleSource source,
                                  Handle handle)
    {
        var modifyingSelection = IsModifyingSelection();

        if (modifyingSelection)
        {
            if (_context.HandleMap.TryGetHandleEntry(handle, out var entry))
            {
                entry.SetSelected(!entry.Selected);
            }
        }
        else
        {
            _context.HandleMap.ClearSelection();

            if (_context.HandleMap.TryGetHandleEntry(handle, out var entry))
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

    private bool IsModifyingSelection()
    {
        return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
    }
    
    private void ActionInvoked(ISheetElementAction action)
    {
        var editContext = new EditSheetElementContext(_sheet, _selection);
        
        action.Invoke(_context, _sheet, _selection);
        
        _operationService.Push(editContext.FlushOperation());
    }
    
    private IEnumerable<ISheetElement> GetEditableSelection()
    {
        return _sheet.Selection;
            //.Where(e => e.HandleSource.Handles.Any());
    }
}

