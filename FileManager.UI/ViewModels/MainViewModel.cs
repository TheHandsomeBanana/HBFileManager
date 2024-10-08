using FileManager.Core.JobSteps;
using FileManager.Core.Workspace;
using FileManager.UI.Models.SettingsModels;
using FileManager.UI.Services.SettingsService;
using HBLibrary.Common;
using HBLibrary.Common.Account;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Plugins;
using HBLibrary.Common.Workspace;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System.Collections.ObjectModel;
using System.Windows;
using Unity;

namespace FileManager.UI.ViewModels;
public class MainViewModel : AsyncInitializerViewModelBase {
    private readonly IUnityContainer container;
    private readonly INavigationStore navigationStore;
    private readonly ISettingsService settingsService;
    private readonly IAccountService accountService;
    private readonly IApplicationStorage applicationStorage;
    private readonly IPluginManager pluginManager;
    private readonly CommonAppSettings commonAppSettings;
    private readonly IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager;
    private WorkspaceLocationCache? workspaceLocationCache;

    public ViewModelBase? CurrentViewModel => navigationStore[nameof(MainViewModel)].ViewModel;

    public NavigateCommand<ExplorerViewModel> NavigateToExplorerCommand { get; set; }
    public NavigateCommand<JobsViewModel> NavigateToJobsCommand { get; set; }
    public NavigateCommand<ScriptingViewModel> NavigateToScriptingCommand { get; set; }
    public NavigateCommand<ExecutionViewModel> NavigateToExecutionCommand { get; set; }
    public NavigateCommand<SettingsViewModel> NavigateToSettingsCommand { get; set; }
    public NavigateCommand<ApplicationLogViewModel> NavigateToApplicationLogCommand { get; set; }
    public NavigateCommand<AboutViewModel> NavigateToAboutCommand { get; set; }
    public NavigateCommand<WorkspacesViewModel> NavigateToWorkspacesCommand { get; set; }
    public string NavigateCommandParameter => nameof(MainViewModel);

    public RelayCommand SaveApplicationStateCommand { get; set; }
    public RelayCommand<Window> OpenAccountOverviewCommand { get; set; }

    public ObservableCollection<string> Workspaces { get; set; } = [];
    private string? selectedWorkspace;

    public string? SelectedWorkspace {
        get {
            return selectedWorkspace;
        }
        set {
            selectedWorkspace = value;
            NotifyPropertyChanged();

            if (selectedWorkspace is not null) {

                SwitchWorkspaceAsync()
                    .ContinueWith(e => {
                        if (e.Result.IsFaulted) {
                            selectedWorkspace = null;
                            NotifyPropertyChanged();

                            HBDarkMessageBox.Show("Workspace switch error",
                                e.Result.Exception!.Message,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    });
            }
        }
    }


    public MainViewModel() {
        container = UnityBase.GetChildContainer(nameof(FileManager))!;

        this.applicationStorage = container.Resolve<IApplicationStorage>();
        this.settingsService = container.Resolve<ISettingsService>();
        this.navigationStore = container.Resolve<INavigationStore>();
        this.accountService = container.Resolve<IAccountService>();
        this.commonAppSettings = container.Resolve<CommonAppSettings>();
        this.pluginManager = container.Resolve<IPluginManager>();
        this.workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();

        INavigationService navigationService = container.Resolve<INavigationService>();

        NavigateToExplorerCommand = new NavigateCommand<ExplorerViewModel>(navigationService, () => new ExplorerViewModel());
        NavigateToJobsCommand = new NavigateCommand<JobsViewModel>(navigationService, () => new JobsViewModel());
        NavigateToScriptingCommand = new NavigateCommand<ScriptingViewModel>(navigationService, () => new ScriptingViewModel());
        NavigateToExecutionCommand = new NavigateCommand<ExecutionViewModel>(navigationService, () => new ExecutionViewModel());
        NavigateToSettingsCommand = new NavigateCommand<SettingsViewModel>(navigationService, () => new SettingsViewModel());
        NavigateToApplicationLogCommand = new NavigateCommand<ApplicationLogViewModel>(navigationService, () => new ApplicationLogViewModel());
        NavigateToAboutCommand = new NavigateCommand<AboutViewModel>(navigationService, () => new AboutViewModel());
        NavigateToWorkspacesCommand = new NavigateCommand<WorkspacesViewModel>(navigationService, () => new WorkspacesViewModel());


        SaveApplicationStateCommand = new RelayCommand(SaveApplicationState, true);
        OpenAccountOverviewCommand = new RelayCommand<Window>(OpenAccountOverview, true);

        NavigateToWorkspacesCommand.Execute(NavigateCommandParameter);
        navigationStore[nameof(MainViewModel)].CurrentViewModelChanged += MainWindowViewModel_CurrentViewModelChanged;
    }

    protected override async Task InitializeViewModelAsync() {
        IWorkspaceLocationManager workspaceLocationManager = container.Resolve<IWorkspaceLocationManager>();
        workspaceLocationCache = await workspaceLocationManager.GetWorkspaceLocationsAsync();
        Workspaces = new ObservableCollection<string>(workspaceLocationCache.WorkspaceLocations);

        if (workspaceLocationCache.LastWorkspace is not null) {
            SelectedWorkspace = workspaceLocationCache.LastWorkspace;
            NavigateToExplorerCommand.Execute(NavigateCommandParameter);
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

    protected override void OnInitializeException(Exception exception) {
        HBDarkMessageBox.Show("Initialization error",
            exception.Message,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void OpenAccountOverview(Window obj) {
        AccountViewModel accountViewModel = new AccountViewModel(obj,
            accountService,
            commonAppSettings, s => UserSwitchCallback(obj, s),
            AppStateHandler.PreventShutdown
        );

        HBDarkAccountWindow accountWindow = new HBDarkAccountWindow(obj, accountViewModel);
        accountWindow.Show();
    }

    private void SaveApplicationState(object? obj) {
        AppStateHandler.SaveAppState();
        HBDarkMessageBox.Show("Saved", "Application state saved successfully.");
    }

    private void MainWindowViewModel_CurrentViewModelChanged() {
        NotifyPropertyChanged(nameof(CurrentViewModel));
    }

    private void UserSwitchCallback(Window obj, bool success) {
        if (success) {
            IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;

            // User changed
            // -> Use new storage containers
            applicationStorage.RemoveAllContainers();
            App.AddApplicationStorageContainers(container);

            ApplicationState appState = new ApplicationState {
                WindowState = obj.WindowState,
                Left = obj.Left,
                Top = obj.Top,
            };

            obj = new MainWindow(appState) {
                DataContext = new MainViewModel(),
            };

            obj.Closing += (_, _) => {
                if (AppStateHandler.CanShutdown) {
                    AppStateHandler.SaveAppStateBeforeExit();
                }
            };

            obj.Closed += (_, _) => {

                if (AppStateHandler.CanShutdown) {
                    Application.Current.Shutdown();
                }
                else {
                    AppStateHandler.AllowShutdown();
                }
            };

            // Change PluginManager folder based on logged in user
            pluginManager.SwitchContext(e => e.SetPluginsLocation(App.GetPluginStoragePath(container)));
            obj.Show();
        }
        else {
            Application.Current.Shutdown();
        }
    }

    private async Task<Result> SwitchWorkspaceAsync() {
        if (workspaceManager.CurrentWorkspace is not null) {
            await workspaceManager.CurrentWorkspace.CloseAsync();
        }

        Result workspaceResult = await workspaceManager.OpenAsync(workspaceLocationCache!.LastWorkspace!, accountService.Account!);
        return workspaceResult;
    }
}
