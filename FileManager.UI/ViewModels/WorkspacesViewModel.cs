using FileManager.Core.Workspace;
using HBLibrary.Common;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Workspace;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xaml.Schema;
using Unity;

namespace FileManager.UI.ViewModels;
public class WorkspacesViewModel : AsyncInitializerViewModelBase {
    private readonly IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager;
    private readonly IWorkspaceLocationManager locationManager;

    public AsyncRelayCommand CreateWorkspaceCommand { get; set; }
    public RelayCommand<HBFileManagerWorkspace> RemoveWorkspaceCommand { get; set; }
    public AsyncRelayCommand<HBFileManagerWorkspace> ExportWorkspaceCommand { get; set; }
    public AsyncRelayCommand ImportWorkspaceCommand { get; set; }

    public ObservableCollection<HBFileManagerWorkspace> Workspaces { get; set; } = [];

    private HBFileManagerWorkspace? selectedWorkspace;
    public HBFileManagerWorkspace? SelectedWorkspace {
        get => selectedWorkspace; 
        set {
            selectedWorkspace = value;
            NotifyPropertyChanged();
        }
    }


    public WorkspacesViewModel() {
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;

        workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        locationManager = container.Resolve<IWorkspaceLocationManager>();

        CreateWorkspaceCommand = new AsyncRelayCommand(CreateWorkspace, _ => true, e => OnException(e, "Workspace create error"));
        RemoveWorkspaceCommand = new RelayCommand<HBFileManagerWorkspace>(RemoveWorkspace, _ => true);
        ExportWorkspaceCommand = new AsyncRelayCommand<HBFileManagerWorkspace>(ExportWorkspace, _ => true, e => OnException(e, "Workspace export error"));
        ImportWorkspaceCommand = new AsyncRelayCommand(ImportWorkspace, _ => true, e => OnException(e, "Workspace import error"));
    }



    protected override async Task InitializeViewModelAsync() {
        WorkspaceLocationCache locationCache = await locationManager.GetWorkspaceLocationsAsync();
        foreach (string location in locationCache.WorkspaceLocations) {
            Result<HBFileManagerWorkspace> workspaceResult = await workspaceManager.GetAsync(location);
            workspaceResult.Tap(Workspaces.Add);
            workspaceResult.TapError(e => OnException(e, "Workspace fetch error"));
        }
    }

    protected override void OnInitializeException(Exception exception) {
        OnException(exception, "Initialization error");
    }

    private async Task ImportWorkspace(object? arg) {
        throw new NotImplementedException();
    }

    private async Task ExportWorkspace(HBFileManagerWorkspace workspace) {
        throw new NotImplementedException();
    }

    private void RemoveWorkspace(HBFileManagerWorkspace workspace) {
        Workspaces.Remove(workspace);
    }

    private async Task CreateWorkspace(object? arg) {
        throw new NotImplementedException();
    }

    private void OnException(Exception e, string title) {
        HBDarkMessageBox.Show(title, e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    
}
