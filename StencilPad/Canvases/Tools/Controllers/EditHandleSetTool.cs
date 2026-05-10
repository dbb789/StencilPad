using System.Windows.Input;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Controllers.Actions;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class EditHandleSetTool : ITool
{
    public class Factory(SheetElementEditActions SheetElementEditActions,
                         IOperationService OperationService) : IToolFactory
    {
        public string IconResource => "EditHandleSetTool";
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
    private readonly SheetElementEditActions _sheetElementEditActions;
    private readonly IOperationService _operationService;
    private readonly List<ISheetElement> _selection;
    
    private EditHandleSetToolOverlay? _overlay;
    private EditSheetElementContext? _editContext;
    
    private EditHandleSetTool(IToolButton button,
                              Sheet sheet,
                              IToolContext context,
                              SheetElementEditActions sheetElementEditActions,
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
        
        _overlay = new EditHandleSetToolOverlay(_sheet,
                                                _context.Viewport,
                                                _context.UnitSnap,
                                                _sheetElementEditActions,
                                                _context.EditOverlayRenderer);
        
        _context.ToolOverlay.ActiveOverlay = _overlay;
        _context.EditOverlayRenderer.IsEnabled = true;

        _overlay.Selection = _selection;
        _overlay.HandleDragBegin += OnHandleDragBegin;
        _overlay.HandleDragged += OnHandleDragged;
        _overlay.HandleDragEnd += OnHandleDragEnd;
        _overlay.HandleSelectionChanged += OnHandleSelectionChanged;
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
            _overlay.HandleSelectionChanged -= OnHandleSelectionChanged;
            _overlay.ActionInvoked -= ActionInvoked;
            _overlay.Dispose();
            _overlay = null;
        }

        _context.RubberBand.BoundsSelected -= OnBoundsSelected;
        _context.RubberBand.PointSelected -= OnPointSelected;
    }

    private void OnHandleDragBegin()
    {
        if (_selection.Count == 0)
        {
            return;
        }
        
        _editContext = new EditSheetElementContext(_sheet, _selection);
    }

    private void OnHandleDragged(ISheetElement element,
                                 Handle handle,
                                 Unit2D delta)
    {
        if (!handle.CanGroupMove)
        {
            element.HandleSet.SetPoint(handle, element.HandleSet.GetPoint(handle) + delta);
            return;
        }
        
        foreach (var e in _selection)
        {
            foreach (var selected in e.HandleSet.GetSelectedHandles())
            {
                if (selected.CanGroupMove)
                {
                    e.HandleSet.SetPoint(selected, e.HandleSet.GetPoint(selected) + delta);
                }
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
    
    private void OnHandleSelectionChanged(ISheetElement element,
                                          Handle handle,
                                          bool selected)
    {
        var list = element.HandleSet.GetSelectedHandles().ToList();

        if (IsModifyingSelection())
        {
            if (selected && !list.Contains(handle))
            {
                list.Add(handle);
            }
            else
            {
                list.Remove(handle);
            }
        }
        else
        {
            foreach (var otherElement in _selection)
            {
                if (otherElement != element)
                {
                    otherElement.HandleSet.SetSelectedHandles([]);
                }
            }
            
            list.Clear();
            
            if (selected)
            {
                list.Add(handle);
            }
        }

        element.HandleSet.SetSelectedHandles(list);
    }

    private void OnBoundsSelected(UnitBounds bounds)
    {
        if (_selection.Count == 0)
        {
            return;
        }

        var modifyingSelection = IsModifyingSelection();
        
        foreach (var element in _selection)
        {
            var handles = element.HandleSet.Handles
                .Where(h => bounds.Contains(element.HandleSet.GetPoint(h)));

            if (modifyingSelection)
            {
                handles = handles.Union(element.HandleSet.GetSelectedHandles());
            }
            
            element.HandleSet.SetSelectedHandles(handles);
        }
    }

    private void OnPointSelected(Unit2D point)
    {
        if (_selection.Count == 0)
        {
            return;
        }
        
        foreach (var element in _selection)
        {
            element.HandleSet.SetSelectedHandles([]);
        }
    }
    
    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        _selection.Clear();
        _selection.AddRange(GetEditableSelection());
        
        _button.IsEnabled = _selection.Count > 0;

        if (_overlay is not null)
        {
            _overlay.Selection = _selection;
        }
    }

    private bool IsModifyingSelection()
    {
        return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
    }
    
    private void ActionInvoked(ISheetElementAction action)
    {
        var editContext = new EditSheetElementContext(_sheet, _selection);
        
        action.Invoke(_sheet, _selection);
        
        _operationService.Push(editContext.FlushOperation());
    }
    
    private IEnumerable<ISheetElement> GetEditableSelection()
    {
        return _sheet.Selection
            .Where(e => e.HandleSet.Handles.Any());
    }
}

