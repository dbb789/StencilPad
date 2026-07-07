using System.Diagnostics;
using System.Windows;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.ViewModels;
using StencilPad.Spatial;

namespace StencilPad.Controllers;

public class AppController
{
    private readonly Project _project;
    private readonly IOperationService _operationService;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private readonly IClipboardService _clipboardService;
    private readonly IFileService _fileService;
    private readonly IImportExportService _importExportService;
    private readonly MainWindowViewModel _viewModel;
    private readonly SheetTabController.Factory _tabControllerFactory;
    private readonly UndoStack _undoStack;

    private List<(SheetTabController Controller, SheetTabViewModel ViewModel)> _sheetTabs = new();
    private string? _currentFilePath;

    public AppController(Project project,
                         MainWindowViewModel viewModel,
                         IOperationService operationService,
                         IDialogService dialogService,
                         IPrintService printService,
                         IClipboardService clipboardService,
                         IFileService fileService,
                         IImportExportService importExportService,
                         SheetTabController.Factory tabControllerFactory)
    {

        _project = project;
        _viewModel = viewModel;
        _operationService = operationService;
        _dialogService = dialogService;
        _printService = printService;
        _clipboardService = clipboardService;
        _fileService = fileService;
        _importExportService = importExportService;
        _tabControllerFactory = tabControllerFactory;
        _undoStack = new();
        
        _viewModel.NewProjectCommand = new RelayCommand(NewProject);
        _viewModel.GridSettingsCommand = new RelayCommand(GridSettings);
        _viewModel.UnitScaleCommand = new RelayCommand(UnitScale);
        _viewModel.AddSheetCommand = new RelayCommand(AddNewSheet);
        _viewModel.RenameSheetCommand = new RelayCommand(RenameActiveSheet);
        _viewModel.DeleteSheetCommand = new RelayCommand(DeleteActiveSheet);
        _viewModel.PrintCommand = new RelayCommand(PrintSelectedTabAsync);
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
        _viewModel.ImportImageCommand = new RelayCommand(ImportImageAsync);
        _viewModel.ExportSvgCommand = new RelayCommand(ExportSvg);
        _viewModel.ExportPngCommand = new RelayCommand(ExportPng);

        _undoStack.SaveStateChanged += UpdateTitle;
        _operationService.OperationPushed += PushOperation;

        _project.SheetAdded += SheetAdded;
        _project.SheetRemoved += SheetRemoved;
    }

    public void Initialize()
    {
        NewProject();
    }

    public bool ConfirmClose()
    {
        if (_undoStack.SaveState)
        {
            return true;
        }

        return _dialogService.ShowConfirmation(
            "You have unsaved changes. Are you sure you want to close without saving?",
            "Unsaved Changes");
    }

    private void NewProject()
    {
        _undoStack.Clear();
        _operationService.DiscardEditContext();
        
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
                _undoStack.Clear();
                _operationService.DiscardEditContext();
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
            _undoStack.MarkSavePoint();
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
                _undoStack.MarkSavePoint();
            }
        }
        catch (FileServiceException ex)
        {
            _dialogService.ShowError(ex.Message, "Cannot Save File");
        }
    }

    private void UnitScale()
    {
        var result = _dialogService.ShowUnitScaleDialog(_project.UnitRatio);

        if (result is null)
        {
            return;
        }

        _project.UnitRatio = result.Value;
    }

    private void GridSettings()
    {
        Unit gridSpacing;
        int gridSubdivisions;
        
        if (_project.UnitSystem == UnitSystem.Metric)
        {
            gridSpacing = _project.GridSpacingMetric;
            gridSubdivisions = _project.GridSubdivisionsMetric;
        }
        else
        {
            gridSpacing = _project.GridSpacingImperial;
            gridSubdivisions = _project.GridSubdivisionsImperial;
        }

        var result = _dialogService.ShowGridSettingsDialog(gridSpacing,
                                                           gridSubdivisions,
                                                           _project.UnitSettings);

        if (result is null)
        {
            return;
        }

        if (_project.UnitSystem == UnitSystem.Metric)
        {
            _project.GridSpacingMetric = result.Value.Spacing;
            _project.GridSubdivisionsMetric = result.Value.Subdivisions;
        }
        else
        {
            _project.GridSpacingImperial = result.Value.Spacing;
            _project.GridSubdivisionsImperial = result.Value.Subdivisions;
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

    private async void PrintSelectedTabAsync()
    {
        var selectedTab = _viewModel.SelectedTab;
        
        if (selectedTab is null)
        {
            _dialogService.ShowWarning("No sheet selected.", "Print Failed");
            return;
        }

        var sheet = selectedTab.Sheet;
        var documentName = selectedTab.Header;
        
        var success = await _printService.PrintAsync(documentName, sheet);

        if (!success)
        {
            _dialogService.ShowWarning("Print job failed or was cancelled.", "Print Failed");
        }
    }

    private void PushOperation(IOperation operation, bool shouldExecute)
    {
        _undoStack.Push(operation);

        if (shouldExecute)
        {
            operation.Execute(_project, out var targetSheet);
        }
    }

    private void Undo()
    {
        if (_operationService.HasEditContext)
        {
            Debug.WriteLine("Trying to undo while an edit context is active");
            return;
        }
        
        _undoStack.Undo(_project, out var targetSheet);

        SelectSheet(targetSheet);
    }

    private void Redo()
    {
        if (_operationService.HasEditContext)
        {
            Debug.WriteLine("Trying to redo while an edit context is active");
            return;
        }

        _undoStack.Redo(_project, out var targetSheet);
        
        SelectSheet(targetSheet);
    }

    private void SelectSheet(Sheet? sheet)
    {
        if (sheet is null)
        {
            return;
        }
        
        var tab = _sheetTabs.FirstOrDefault(t => t.ViewModel.Sheet == sheet);

        if (tab.ViewModel is not null)
        {
            _viewModel.SelectedTab = tab.ViewModel;
        }
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

        if (sheet is null)
        {
            return;
        }

        _clipboardService.Copy(sheet);
    }

    private void CutToClipboard()
    {
        var sheet = _viewModel.SelectedTab?.Sheet;

        if (sheet is null)
        {
            return;
        }

        _clipboardService.Cut(sheet);
    }

    private void PasteFromClipboard()
    {
        var sheet = _viewModel.SelectedTab?.Sheet;

        if (sheet is null)
        {
            return;
        }

        _clipboardService.Paste(sheet);
    }
    
    private async void ImportImageAsync()
    {
        var tab = _viewModel.SelectedTab;
        
        if (tab is null)
        {
            return;
        }

        var viewport = tab.Viewport;

        if (viewport is not null)
        {
            await _importExportService.ImportImageAsync(tab.Sheet, viewport);
        }
    }

    private void ExportSvg()
    {
        var tab = _viewModel.SelectedTab;
        
        if (tab is null)
        {
            return;
        }

        var sheet = tab.Sheet;

        _importExportService.ExportSvg(sheet);
    }

    private void ExportPng()
    {
        var tab = _viewModel.SelectedTab;
        
        if (tab is null)
        {
            return;
        }

        var sheet = tab.Sheet;

        _importExportService.ExportPng(sheet);
    }

    private void SetCurrentFilePath(string? path)
    {
        _currentFilePath = path;
        UpdateTitle();
    }

    private void UpdateTitle()
    {
        var title = _currentFilePath is not null
            ? $"{System.IO.Path.GetFileName(_currentFilePath)} - StencilPad"
            : "StencilPad";

        if (!_undoStack.SaveState)
        {
            title += " *";
        }

        _viewModel.Title = title;
    }
}
