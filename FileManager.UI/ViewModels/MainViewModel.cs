using FileManager.Core.JobSteps;
using FileManager.Core.Workspace;
using FileManager.UI.Models.SettingsModels;
using FileManager.UI.Services.SettingsService;
using HBLibrary.Common;
using HBLibrary.Core;
using HBLibrary.Core.Extensions;
using HBLibrary.DataStructures;
using HBLibrary.DI;
using HBLibrary.Interface.Core.ChangeTracker;
using HBLibrary.Interface.IO.Storage;
using HBLibrary.Interface.IO.Storage.Container;
using HBLibrary.Interface.Plugins;
using HBLibrary.Interface.Security.Account;
using HBLibrary.Interface.Workspace;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Controls;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using Unity;

namespace FileManager.UI.ViewModels;
public class MainViewModel : AsyncInitializerViewModelBase, IDisposable {
    private readonly IUnityContainer container;
    private readonly INavigationService navigationService;
    private readonly INavigationStore navigationStore;
    private readonly ISettingsService settingsService;
    private readonly IAccountService accountService;
    private readonly IPluginManager pluginManager;
    private readonly CommonAppSettings commonAppSettings;
    private readonly IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager;
    private readonly IWorkspaceLocationManager workspaceLocationManager;

    public ViewModelBase? CurrentViewModel => navigationStore[nameof(MainViewModel)].ViewModel;
    public NavigateCommand<ExplorerViewModel> NavigateToExplorerCommand { get; set; }
    public NavigateCommand<JobsViewModel> NavigateToJobsCommand { get; set; }
    public NavigateCommand<ScriptingViewModel> NavigateToScriptingCommand { get; set; }
    public NavigateCommand<ExecutionViewModel> NavigateToExecutionCommand { get; set; }
    public NavigateCommand<SettingsViewModel> NavigateToSettingsCommand { get; set; }
    public NavigateCommand<ApplicationLogViewModel> NavigateToApplicationLogCommand { get; set; }
    public NavigateCommand<AboutViewModel> NavigateToAboutCommand { get; set; }
    public RelayCommand NavigateToWorkspacesCommand { get; set; }
    public string NavigateCommandParameter => nameof(MainViewModel);



    private ListBoxButton? navigationIndex;
    public ListBoxButton? NavigationIndex {
        get => navigationIndex;
        set {
            navigationIndex = value;
            NotifyPropertyChanged();
        }
    }

    public RelayCommand SaveApplicationStateCommand { get; set; }
    public RelayCommand<Window> OpenAccountOverviewCommand { get; set; }
    public RelayCommand OpenNotificationsCommand { get; set; }

    public ObservableCollection<WorkspaceLocationViewModel> Workspaces { get; set; } = [];

    private WorkspaceLocationViewModel? selectedWorkspace;
    public WorkspaceLocationViewModel? SelectedWorkspace {
        get {
            return selectedWorkspace;
        }
        set {
            if (workspaceManager.CurrentWorkspace?.ChangeTracker.HasActiveChanges ?? false) {
                MessageBoxResult result = HBDarkMessageBox.Show("Unsaved changes",
                    "This workspace contains unsaved changes. Save them now?",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning);

                // Workspace is saved on switch already..
                if (result == MessageBoxResult.Cancel) {
                    Application.Current.Dispatcher.Invoke(() => {
                        NotifyPropertyChanged(nameof(SelectedWorkspace));
                    });
                    return;
                }

                if (result == MessageBoxResult.OK) {
                    if (SelectedWorkspace is not null) {
                        SelectedWorkspace.Name = SelectedWorkspace.Name.TrimEnd(' ', '*');
                    }

                    Application.Current.Dispatcher.Invoke(() => {
                        NotifyPropertyChanged(nameof(SelectedWorkspace));
                    });
                }
            }

            if (value is not null) {
                selectedWorkspace = value;
                SwitchWorkspaceAsync(selectedWorkspace.Model)
                    .ContinueWith(e => {
                        if (e.Result.IsSuccess) {
                            workspaceManager.CurrentWorkspace!.ChangeTracker.ChangeTrackerStateChanged += OnWorkspaceChangeTrackerStateChanged;

                            workspaceLocationManager.SetLatestWorkspaceLocation(selectedWorkspace.Model);
                            NotifyPropertyChanged(nameof(SelectedWorkspace));

                            Application.Current.Dispatcher.Invoke(() => {
                                if (NavigationIndex is not null) {
                                    // Trigger page refresh
                                    NavigateCommand<WorkspacesViewModel> navigateCommand = GetNavigateWorkspacesCommand();
                                    navigateCommand.Execute(NavigateCommandParameter);
                                    NavigationIndex.Command.Execute(NavigateCommandParameter);
                                }
                            });
                        }
                        else if (e.Result.IsFaulted) {
                            selectedWorkspace = null;
                            NotifyPropertyChanged(nameof(SelectedWorkspace));

                            Application.Current.Dispatcher.Invoke(() => {
                                HBDarkMessageBox.Show("Workspace load error",
                                    e.Result.Exception!.Message,
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            });
                        }
                    });
            }
        }
    }

    private string title = "HB File Manager";

    public string Title {
        get { return title; }
        set {
            title = value;
            NotifyPropertyChanged();
        }
    }

    public MainViewModel() {
        container = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);

        this.navigationStore = container.Resolve<INavigationStore>();
        this.accountService = container.Resolve<IAccountService>();
        this.commonAppSettings = container.Resolve<CommonAppSettings>();
        this.pluginManager = container.Resolve<IPluginManager>();
        this.workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        this.navigationService = container.Resolve<INavigationService>();
        this.workspaceLocationManager = container.Resolve<IWorkspaceLocationManager>();
        this.settingsService = container.Resolve<ISettingsService>();

        NavigateToExplorerCommand = new NavigateCommand<ExplorerViewModel>(navigationService, () => new ExplorerViewModel());
        NavigateToJobsCommand = new NavigateCommand<JobsViewModel>(navigationService, () => new JobsViewModel());
        NavigateToScriptingCommand = new NavigateCommand<ScriptingViewModel>(navigationService, () => new ScriptingViewModel());
        NavigateToExecutionCommand = new NavigateCommand<ExecutionViewModel>(navigationService, () => new ExecutionViewModel());
        NavigateToSettingsCommand = new NavigateCommand<SettingsViewModel>(navigationService, () => new SettingsViewModel());
        NavigateToApplicationLogCommand = new NavigateCommand<ApplicationLogViewModel>(navigationService, () => new ApplicationLogViewModel());
        NavigateToAboutCommand = new NavigateCommand<AboutViewModel>(navigationService, () => new AboutViewModel());
        NavigateToWorkspacesCommand = new RelayCommand(NavigateToWorkspaces);

        SaveApplicationStateCommand = new RelayCommand(SaveApplicationState);
        OpenAccountOverviewCommand = new RelayCommand<Window>(OpenAccountOverview);
        OpenNotificationsCommand = new RelayCommand(OpenNotifications);

        navigationStore[nameof(MainViewModel)].CurrentViewModelChanged += MainWindowViewModel_CurrentViewModelChanged;

        workspaceLocationManager.WorkspaceLocationsChanged += WorkspaceLocationManager_WorkspaceLocationsChanged;
    }

    private void OpenNotifications(object? obj) {
        throw new NotImplementedException();
    }

    protected override async Task InitializeViewModelAsync() {
        IApplicationStorage mainStorage = container.Resolve<IApplicationStorage>();
        foreach (IStorageEntryContainer container in mainStorage.GetContainers()) {
            if (container.ChangeTracker is not null) {
                container.ChangeTracker.ChangeTrackerStateChanged += OnChangeTrackerStateChanged;
                container.ChangeTracker.HookStateChanged();
            }
        }


        WorkspaceLocationCache locationCache = workspaceLocationManager.LocationCache;
        bool updateRequired = false;
        foreach (WorkspaceLocation location in locationCache.WorkspaceLocations.ToArray()) {
            if (!System.IO.File.Exists(location.FullPath)) {
                locationCache.WorkspaceLocations.Remove(location);
                updateRequired = true;
            }
        }

        if (!System.IO.File.Exists(locationCache.LastWorkspace?.FullPath)) {
            locationCache.LastWorkspace = null;
            updateRequired = true;
        }

        if (updateRequired) {
            workspaceLocationManager.Update(locationCache);
        }


        foreach (WorkspaceLocation location in locationCache.WorkspaceLocations) {
            Result<HBFileManagerWorkspace> workspaceGetResult = await workspaceManager.GetAsync(location.FullPath!, accountService.Account!);

            if (workspaceGetResult.IsSuccess) {
                Workspaces.Add(new WorkspaceLocationViewModel(location));
            }
        }

        if (locationCache.LastWorkspace is not null && Workspaces.Any(e => e.Model == locationCache.LastWorkspace)) {
            SelectedWorkspace = Workspaces.First(e => e.Model == locationCache.LastWorkspace);
            NavigateToExplorerCommand.Execute(NavigateCommandParameter);
        }
        else {
            NavigateToWorkspacesCommand.Execute(NavigateCommandParameter);
        }

        await Application.Current.Dispatcher.InvokeAsync(() => {
            ImmutableResultCollection loadResult = pluginManager.LoadAssemblies();
            if (loadResult.IsFaulted) {
                foreach (Result result in loadResult.Where(e => e.IsFaulted)) {
                    HBDarkMessageBox.Show("Plugin load error",
                        result.Exception!.Message,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        });
    }

    private void OnChangeTrackerStateChanged(bool state) {
        if (state) {
            Title = "HB File Manager *";
        }
        else {
            Title = "HB File Manager";
        }
    }

    protected override void OnInitializeException(Exception exception) {
        HBDarkMessageBox.Show("Initialization error",
            exception.Message,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void OpenAccountOverview(Window obj) {

        AccountViewModel accountViewModel = new AccountViewModel(obj,
            accountService, commonAppSettings,
            ApplicationHandler.OnAccountSwitched,
            ApplicationHandler.OnAccountSwitching
        );

        HBDarkAccountWindow accountWindow = new HBDarkAccountWindow(obj, accountViewModel);
        accountWindow.Show();
    }

    private void SaveApplicationState(object? obj) {
        ApplicationHandler.SaveAppState();
        HBDarkMessageBox.Show("Saved", "Application state saved successfully.");
    }

    private void NavigateToWorkspaces(object? obj) {
        NavigateCommand<WorkspacesViewModel> navigateCommand = GetNavigateWorkspacesCommand();
        navigateCommand.Execute(NavigateCommandParameter);
        NavigationIndex = null;
    }

    private void MainWindowViewModel_CurrentViewModelChanged() {
        NotifyPropertyChanged(nameof(CurrentViewModel));
    }

    private void OnWorkspaceChangeTrackerStateChanged(bool obj) {
        Application.Current.Dispatcher.Invoke(() => {
            if (obj) {
                if (SelectedWorkspace is not null && !SelectedWorkspace.Name.EndsWith(" *")) {
                    SelectedWorkspace.Name += " *";
                    NotifyPropertyChanged(nameof(SelectedWorkspace));
                }
            }
            else {
                if (SelectedWorkspace is not null) {
                    SelectedWorkspace.Name = SelectedWorkspace.Name.TrimEnd(' ', '*');
                    NotifyPropertyChanged(nameof(SelectedWorkspace));
                }
            }
        });
    }

    private async Task<Result> SwitchWorkspaceAsync(WorkspaceLocation location) {
        HBFileManagerWorkspace? temp = workspaceManager.CurrentWorkspace;

        if (temp is not null && temp.IsOpen && temp.FullPath == location.FullPath) {
            return Result.Ok();
        }


        Result workspaceResult = await workspaceManager.OpenAsync(location!.FullPath, accountService.Account!);

        if (workspaceResult.IsSuccess && temp is not null) {
            await temp.CloseAsync();
            temp.ChangeTracker.ChangeTrackerStateChanged -= OnWorkspaceChangeTrackerStateChanged;
        }

        return workspaceResult;
    }

    private void WorkspaceLocationManager_WorkspaceLocationsChanged(bool added, WorkspaceLocation[] locations) {
        if (!added) {
            if (locations.Contains(SelectedWorkspace?.Model)) {
                workspaceManager.CurrentWorkspace?.CloseAsync()
                .FireAndForget();
            }

            foreach (WorkspaceLocation location in locations) {
                Workspaces.RemoveAt(Workspaces.IndexOf(Workspaces.First(e => e.Model == location)));
            }
        }
        else if (added) {
            foreach (WorkspaceLocation location in locations) {
                Workspaces.Add(new WorkspaceLocationViewModel(location));
            }
        }
    }

    private NavigateCommand<WorkspacesViewModel> GetNavigateWorkspacesCommand() {
        return new NavigateCommand<WorkspacesViewModel>(navigationService, () => new WorkspacesViewModel());
    }

    public void Dispose() {
        workspaceLocationManager.WorkspaceLocationsChanged -= WorkspaceLocationManager_WorkspaceLocationsChanged;
        navigationStore[nameof(MainViewModel)].CurrentViewModelChanged -= MainWindowViewModel_CurrentViewModelChanged;
        navigationStore.Dispose();
    }
}
