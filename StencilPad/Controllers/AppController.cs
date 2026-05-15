using System.IO;
using System.Windows;
using Microsoft.Win32;
using StencilPad.Canvases.Rendering;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;
using StencilPad.ViewModels;

namespace StencilPad.Controllers;

public class AppController
{
    public class Factory(IOperationService OperationService,
                         IDialogService DialogService,
                         IPrintService PrintService,
                         IClipboardService ClipboardService,
                         IFileService FileService,
                         SheetTabController.Factory TabControllerFactory)
    {
        public AppController Create(Project project,
                                    MainWindowViewModel viewModel)
        {
            return new(project,
                       viewModel,
                       OperationService,
                       DialogService,
                       PrintService,
                       ClipboardService,
                       FileService,
                       TabControllerFactory);
        }
    }
    
    private readonly Project _project;
    private readonly IOperationService _operationService;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private readonly IClipboardService _clipboardService;
    private readonly IFileService _fileService;
    private readonly MainWindowViewModel _viewModel;
    private readonly SheetTabController.Factory _tabControllerFactory;
    private readonly UndoStack _undoStack;

    private List<(SheetTabController Controller, SheetTabViewModel ViewModel)> _sheetTabs = new();
    private string? _currentFilePath;

    private Unit2D _pasteMajorOffset = new Unit2D(Unit.FromMillimeters(-5),
                                                  Unit.FromMillimeters(5));
    
    private Unit2D _pasteMinorOffset = new Unit2D(Unit.FromMillimeters(5),
                                                  Unit.FromMillimeters(5));
    private int _pasteCounter;

    private AppController(Project project,
                          MainWindowViewModel viewModel,
                          IOperationService operationService,
                          IDialogService dialogService,
                          IPrintService printService,
                          IClipboardService clipboardService,
                          IFileService fileService,
                          SheetTabController.Factory tabControllerFactory)
    {
        _project = project;
        _viewModel = viewModel;
        _operationService = operationService;
        _dialogService = dialogService;
        _printService = printService;
        _clipboardService = clipboardService;
        _fileService = fileService;
        _tabControllerFactory = tabControllerFactory;
        _undoStack = new();
        
        _viewModel.NewProjectCommand = new RelayCommand(NewProject);
        _viewModel.AddSheetCommand = new RelayCommand(AddNewSheet);
        _viewModel.RenameSheetCommand = new RelayCommand(RenameActiveSheet);
        _viewModel.DeleteSheetCommand = new RelayCommand(DeleteActiveSheet);
        _viewModel.PrintCommand = new RelayCommand(PrintSelectedTab);
        _viewModel.ExitCommand = new RelayCommand(() => Application.Current.Shutdown());
        
        _viewModel.OpenProjectCommand = new RelayCommand(async () => await OpenProject());
        _viewModel.SaveProjectCommand = new RelayCommand(async () => await SaveProject());
        _viewModel.SaveProjectAsCommand = new RelayCommand(async () => await SaveProjectAs());
        _viewModel.CopyCommand = new RelayCommand(CopyToClipboard);
        _viewModel.CutCommand = new RelayCommand(CutToClipboard);
        _viewModel.PasteCommand = new RelayCommand(PasteFromClipboard);
        _viewModel.DeleteCommand = new RelayCommand(DeleteSelection);
        _viewModel.UndoCommand = new RelayCommand(Undo);
        _viewModel.RedoCommand = new RelayCommand(Redo);
        _viewModel.ImportImageCommand = new RelayCommand(ImportImage);

        _operationService.OperationPushed += PushOperation;

        _project.SheetAdded += SheetAdded;
        _project.SheetRemoved += SheetRemoved;
    }

    public void Initialize()
    {
        NewProject();
    }

    private void NewProject()
    {
        _project.Clear();
        SetCurrentFilePath(null);
        AddNewSheet();
    }

    private async Task OpenProject()
    {
        try
        {
            var path = await _fileService.OpenAsync(_project);

            if (path is not null)
            {
                SetCurrentFilePath(path);
            }
        }
        catch (FileServiceException ex)
        {
            _dialogService.ShowError(ex.Message, "Cannot Open File");
        }
    }

    private async Task SaveProject()
    {
        if (_currentFilePath is null)
        {
            await SaveProjectAs();
            return;
        }

        try
        {
            await _fileService.SaveAsync(_project, _currentFilePath);
        }
        catch (FileServiceException ex)
        {
            _dialogService.ShowError(ex.Message, "Cannot Save File");
        }
    }

    private async Task SaveProjectAs()
    {
        try
        {
            var path = await _fileService.SaveAsAsync(_project);

            if (path is not null)
            {
                SetCurrentFilePath(path);
            }
        }
        catch (FileServiceException ex)
        {
            _dialogService.ShowError(ex.Message, "Cannot Save File");
        }
    }

    private void SheetAdded(Sheet sheet)
    {
        var tabViewModel = new SheetTabViewModel(sheet);
        var tabController = _tabControllerFactory.Create(tabViewModel);

        _sheetTabs.Add((tabController, tabViewModel));
        _viewModel.Tabs.Add(tabViewModel);
        _viewModel.SelectedTab = tabViewModel;
    }

    private void SheetRemoved(Sheet sheet)
    {
        var tabToRemove = _sheetTabs.FirstOrDefault(t => t.ViewModel.Sheet == sheet);

        if (tabToRemove.ViewModel is null)
        {
            return;
        }
        
        _viewModel.Tabs.Remove(tabToRemove.ViewModel);
        _sheetTabs.Remove(tabToRemove);
        
        if (_viewModel.SelectedTab == tabToRemove.ViewModel)
        {
            _viewModel.SelectedTab = _viewModel.Tabs.FirstOrDefault();
        }

        tabToRemove.Controller.Dispose();
        tabToRemove.ViewModel.Dispose();
    }

    private void AddNewSheet()
    {
        var sheet = new Sheet { Name = $"Sheet {_project.Sheets.Count() + 1}" };
        
        _project.AddSheet(sheet);
    }

    private void RenameActiveSheet()
    {
        var selectedSheet = _viewModel.SelectedTab?.Sheet;

        if (selectedSheet is null)
        {
            return;
        }
        
        var newName = _dialogService.ShowRenameDialog(selectedSheet.Name);
        
        if (newName != null)
        {
            selectedSheet.Name = newName;
        }
    }

    private void DeleteActiveSheet()
    {
        var selectedSheet = _viewModel.SelectedTab?.Sheet;

        if (selectedSheet is null)
        {
            return;
        }
        
        if (_project.Sheets.Count() <= 1)
        {
            _dialogService.ShowWarning("A project must contain at least one sheet.",
                                       "Cannot Delete");
            return;
        }

        _project.RemoveSheet(selectedSheet);
    }

    private async void PrintSelectedTab()
    {
        var selectedTab = _viewModel.SelectedTab;

        if (selectedTab?.Sheet is null)
        {
            _dialogService.ShowWarning("No sheet selected.", "Print Failed");
            return;
        }

        var renderers = CreateRenderers(selectedTab.Sheet);

        if (!renderers.Any())
        {
            _dialogService.ShowWarning("No content to print on this sheet.", "Print Failed");
            return;
        }

        var documentName = selectedTab.Header;
        var success = await _printService.PrintAsync(documentName, (dc) =>
        {
            foreach (var renderer in renderers)
            {
                renderer.Render(dc);
            }
        });

        if (!success)
        {
            _dialogService.ShowWarning("Print job failed or was cancelled.", "Print Failed");
        }
    }

    private IEnumerable<SheetElementRenderer> CreateRenderers(Sheet sheet)
    {
        var renderers = new List<SheetElementRenderer>();

        foreach (var element in sheet.Elements)
        {
            var renderer = SheetElementRendererFactory.Create(element);

            if (renderer is not null)
            {
                renderers.Add(renderer);
            }
        }

        return renderers;
    }

    private void PushOperation(IOperation operation, bool shouldExecute)
    {
        _undoStack.Push(operation);

        if (shouldExecute)
        {
            operation.Execute(_project);
        }
    }

    private void Undo()
    {
        _undoStack.Undo(_project);
    }

    private void Redo()
    {
        _undoStack.Redo(_project);
    }

    private void DeleteSelection()
    {
        var tab = _viewModel.SelectedTab;

        if (tab is null || tab.Sheet.Selection.Count == 0)
        {
            return;
        }

        var operations = tab.Sheet.Selection
            .Select(e => new RemoveSheetElementOperation(tab.Sheet, e));

        _operationService.Push(new BulkCommandOperation(operations));
    }

    private void CopyToClipboard()
    {
        var sheet = _viewModel.SelectedTab?.Sheet;

        if (sheet is null || sheet.Selection.Count == 0)
        {
            return;
        }

        _pasteCounter = 0;
        _clipboardService.Copy(sheet.Selection);
    }

    private void CutToClipboard()
    {
        var tab = _viewModel.SelectedTab;

        if (tab is null || tab.Sheet.Selection.Count == 0)
        {
            return;
        }

        _pasteCounter = 0;
        _clipboardService.Copy(tab.Sheet.Selection);

        var operations = tab.Sheet.Selection
            .Select(e => new RemoveSheetElementOperation(tab.Sheet, e));

        _operationService.Push(new BulkCommandOperation(operations));
    }

    private void PasteFromClipboard()
    {
        var tab = _viewModel.SelectedTab;

        if (tab is null)
        {
            return;
        }

        var elements = _clipboardService.Paste();

        if (elements.Count == 0)
        {
            return;
        }

        ++_pasteCounter;
        
        var pasteOffset = _pasteMajorOffset * (_pasteCounter / 10);
        
        pasteOffset += _pasteMinorOffset * (_pasteCounter % 10);
        
        foreach (var element in elements)
        {
            element.Translate(pasteOffset);
        }

        var operations = elements
            .Select(e => new AddSheetElementOperation(tab.Sheet.Id, e));

        _operationService.Push(new BulkCommandOperation(operations));
    }
    
    private void ImportImage()
    {
        var tab = _viewModel.SelectedTab;
        if (tab is null)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = "Import Image",
            Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        byte[] imageData;
        try
        {
            imageData = File.ReadAllBytes(dialog.FileName);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Could not read image file: {ex.Message}", "Import Failed");
            return;
        }

        var size = ImageElementRenderer.MeasureNaturalSize(imageData);
        var halfSize = size / 2.0;

        Unit2D center;
        var viewport = tab.Viewport;
        if (viewport is not null)
        {
            center = viewport.Size / 2.0;
        }
        else
        {
            center = new Unit2D(Unit.FromMillimeters(75), Unit.FromMillimeters(105));
        }

        var element = new ImageElement(center - halfSize, center + halfSize, imageData);

        _operationService.Push(new AddSheetElementOperation(tab.Sheet.Id, element));
    }

    private void SetCurrentFilePath(string? path)
    {
        _currentFilePath = path;
        _viewModel.Title = path is not null
            ? $"{System.IO.Path.GetFileName(path)} - StencilPad"
            : "StencilPad";
    }
}
