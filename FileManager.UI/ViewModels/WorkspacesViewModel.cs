using FileManager.Core.Job;
using FileManager.Core.JobSteps;
using FileManager.Core.Workspace;
using FileManager.UI.ViewModels.JobViewModels;
using FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
using FileManager.UI.ViewModels.WorkspaceViewModels;
using FileManager.UI.Views;
using FileManager.UI.Views.JobViews.JobStepViews;
using FileManager.UI.Views.WorkspaceViews;
using HBLibrary.DataStructures;
using HBLibrary.DI;
using HBLibrary.Interface.Security.Account;
using HBLibrary.Interface.Workspace;
using HBLibrary.Workspace.Exceptions;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xaml.Schema;
using Unity;

namespace FileManager.UI.ViewModels;
public class WorkspacesViewModel : AsyncInitializerViewModelBase, IDisposable {
    private readonly IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager;
    private readonly IWorkspaceLocationManager locationManager;
    private readonly IAccountService accountService;
    private readonly IUnityContainer container;

    public AsyncRelayCommand CreateWorkspaceCommand { get; set; }
    public RelayCommand<WorkspaceItemViewModel> RemoveWorkspaceCommand { get; set; }
    public AsyncRelayCommand<WorkspaceItemViewModel> ExportWorkspaceCommand { get; set; }
    public AsyncRelayCommand ImportWorkspaceCommand { get; set; }

    public ObservableCollection<WorkspaceItemViewModel> Workspaces { get; set; } = [];

    private WorkspaceItemViewModel? selectedWorkspace;
    public WorkspaceItemViewModel? SelectedWorkspace {
        get => selectedWorkspace;
        set {
            selectedWorkspace = value;
            NotifyPropertyChanged();
        }
    }

    private readonly ICollectionView workspacesView;
    public ICollectionView WorkspacesView => workspacesView;

    private string? searchText;
    public string? SearchText {
        get => searchText;
        set {
            searchText = value;
            NotifyPropertyChanged();
            workspacesView.Refresh();
        }
    }

    public WorkspacesViewModel() {
        container = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid)!;

        workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        locationManager = container.Resolve<IWorkspaceLocationManager>();
        accountService = container.Resolve<IAccountService>();

        CreateWorkspaceCommand = new AsyncRelayCommand(CreateWorkspace, _ => true, e => OnException(e, "Workspace create error"));
        RemoveWorkspaceCommand = new RelayCommand<WorkspaceItemViewModel>(RemoveWorkspace, CanRemoveWorkspace);
        ExportWorkspaceCommand = new AsyncRelayCommand<WorkspaceItemViewModel>(ExportWorkspace, _ => true, e => OnException(e, "Workspace export error"));
        ImportWorkspaceCommand = new AsyncRelayCommand(ImportWorkspace, _ => true, e => OnException(e, "Workspace import error"));

        workspacesView = CollectionViewSource.GetDefaultView(Workspaces);
        workspacesView.Filter = FilterWorkspaces;
        workspacesView.CollectionChanged += WorkspacesView_CollectionChanged;

        workspaceManager.OnWorkspaceOpened += OnWorkspaceOpened;
    }

    protected override async Task InitializeViewModelAsync() {
        foreach (WorkspaceLocation location in locationManager.LocationCache.WorkspaceLocations) {
            Result<HBFileManagerWorkspace> workspaceGetResult;
            if (location.FullPath == workspaceManager.CurrentWorkspace?.FullPath) {
                workspaceGetResult = Result<HBFileManagerWorkspace>.Ok(workspaceManager.CurrentWorkspace);
            }
            else {
                workspaceGetResult = await workspaceManager.GetAsync(location.FullPath, accountService.Account!);
            }

            workspaceGetResult.Tap(e => {
                WorkspaceItemViewModel workspaceViewModel = new WorkspaceItemViewModel(e);
                Workspaces.Add(workspaceViewModel);
            });

            workspaceGetResult.TapError(e => {
                if (e is ApplicationWorkspaceException aex && aex.ExceptionType == ApplicationWorkspaceExceptionType.AccessDenied) {
                    return;
                }

                OnException(e, "Workspace get error");
            });
        }
    }

    protected override void OnInitializeException(Exception exception) {
        OnException(exception, "Initialization error");
    }

    private async Task ImportWorkspace(object? arg) {
        OpenFileDialog ofd = new OpenFileDialog {
            Filter = "FM Workspace Files (*.fmws)|*.fmws",
            DefaultExt = ".fmws",
            Title = "Select a HB File Manager Workspace File",
            Multiselect = false
        };

        if (ofd.ShowDialog().GetValueOrDefault()) {
            if (Workspaces.Any(e => e.FullPath == ofd.FileName)) {
                HBDarkMessageBox.Show("Workspace import error",
                    "Workspace already imported",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            Result<HBFileManagerWorkspace> workspaceGetResult = await workspaceManager.GetAsync(ofd.FileName, accountService.Account!);

            workspaceGetResult.Tap(e => {
                WorkspaceItemViewModel workspaceViewModel = new WorkspaceItemViewModel(e);
                Workspaces.Add(workspaceViewModel);
                locationManager.AddWorkspaceLocations([e.FullPath!]);
            });

            workspaceGetResult.TapError(e => {
                OnException(e, "Workspace import error");
            });
        }
    }

    private Task ExportWorkspace(WorkspaceItemViewModel workspace) {
        throw new NotImplementedException();
    }


    private bool CanRemoveWorkspace(WorkspaceItemViewModel? obj) {
        if(obj is null) {
            return false;
        }

        if(!workspaceManager.IsOwner(obj.Model, accountService.Account!)) {
            return false;
        }

        if (workspaceManager.CurrentWorkspace is null) {
            return true;
        }

        return workspaceManager.CurrentWorkspace.FullPath != obj.FullPath;
    }

    private void RemoveWorkspace(WorkspaceItemViewModel workspace) {
        Workspaces.Remove(workspace);

        locationManager.RemoveWorkspaceLocations([workspace.Model.FullPath!]);
    }

    private async Task CreateWorkspace(object? arg) {
        IDialogService dialogService = container.Resolve<IDialogService>();

        AddWorkspaceViewModel addWorkspaceViewModel = new AddWorkspaceViewModel();
        AddWorkspaceView addWorkspaceView = new AddWorkspaceView();

        bool result = dialogService.ShowCompactDialog(addWorkspaceView, addWorkspaceViewModel, "Add Workspace");
        if (result == true) {
            string fullPath = Path.Combine(addWorkspaceViewModel.Directory, addWorkspaceViewModel.Name + HBFileManagerWorkspace.WorkspaceExtension);

            Result<HBFileManagerWorkspace> workspaceCreateResult = addWorkspaceViewModel.UsesEncryption
                ? await workspaceManager.CreateEncryptedAsync(fullPath, accountService.Account!)
                : await workspaceManager.CreateAsync(fullPath, accountService.Account!);

            workspaceCreateResult.Tap(e => {
                WorkspaceItemViewModel workspaceViewModel = new WorkspaceItemViewModel(e);
                Workspaces.Add(workspaceViewModel);
                locationManager.AddWorkspaceLocations([fullPath]);
            });
            workspaceCreateResult.TapError(e => OnException(e, "Workspace create error"));
        }
    }

    private void OnException(Exception e, string title) {
        HBDarkMessageBox.Show(title, e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void WorkspacesView_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        SelectedWorkspace = workspacesView.Cast<WorkspaceItemViewModel>().FirstOrDefault();
    }

    private bool FilterWorkspaces(object obj) {
        if (obj is WorkspaceItemViewModel workspace) {
            return string.IsNullOrEmpty(SearchText) || workspace.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private void OnWorkspaceOpened() {
        RemoveWorkspaceCommand.NotifyCanExecuteChanged();
    }

    public void Dispose() {
        workspaceManager.OnWorkspaceOpened -= OnWorkspaceOpened;
        WorkspacesView.CollectionChanged -= WorkspacesView_CollectionChanged;
        Workspaces.Clear();
    }
}
