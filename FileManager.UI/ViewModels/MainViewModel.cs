using FileManager.Core.JobSteps;
using FileManager.UI.Models.SettingsModels;
using FileManager.UI.Services.SettingsService;
using HBLibrary.Common;
using HBLibrary.Common.Account;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Plugins;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System.Windows;
using Unity;

namespace FileManager.UI.ViewModels;
public class MainViewModel : ViewModelBase {
    private readonly INavigationStore navigationStore;
    private readonly ISettingsService settingsService;
    private readonly IAccountService accountService;
    private readonly IApplicationStorage applicationStorage;
    private readonly IPluginManager pluginManager;
    private readonly CommonAppSettings commonAppSettings;
    public ViewModelBase CurrentViewModel => navigationStore[nameof(MainViewModel)].ViewModel;

    public NavigateCommand<ExplorerViewModel> NavigateToExplorerCommand { get; set; }
    public NavigateCommand<JobsViewModel> NavigateToJobsCommand { get; set; }
    public NavigateCommand<ScriptingViewModel> NavigateToScriptingCommand { get; set; }
    public NavigateCommand<ExecutionViewModel> NavigateToExecutionCommand { get; set; }
    public NavigateCommand<SettingsViewModel> NavigateToSettingsCommand { get; set; }
    public NavigateCommand<ApplicationLogViewModel> NavigateToApplicationLogCommand { get; set; }
    public NavigateCommand<AboutViewModel> NavigateToAboutCommand { get; set; }

    public string NavigateCommandParameter => nameof(MainViewModel);

    public RelayCommand SaveApplicationStateCommand { get; set; }
    public RelayCommand<Window> OpenAccountOverviewCommand { get; set; }

    public MainViewModel() {
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;

        this.applicationStorage = container.Resolve<IApplicationStorage>();
        this.settingsService = container.Resolve<ISettingsService>();
        this.navigationStore = container.Resolve<INavigationStore>();
        this.accountService = container.Resolve<IAccountService>();
        this.commonAppSettings = container.Resolve<CommonAppSettings>();
        this.pluginManager = container.Resolve<IPluginManager>();

        INavigationService navigationService = container.Resolve<INavigationService>();

        NavigateToExplorerCommand = new NavigateCommand<ExplorerViewModel>(navigationService, () => new ExplorerViewModel());
        NavigateToJobsCommand = new NavigateCommand<JobsViewModel>(navigationService, () => new JobsViewModel());
        NavigateToScriptingCommand = new NavigateCommand<ScriptingViewModel>(navigationService, () => new ScriptingViewModel());
        NavigateToExecutionCommand = new NavigateCommand<ExecutionViewModel>(navigationService, () => new ExecutionViewModel());
        NavigateToSettingsCommand = new NavigateCommand<SettingsViewModel>(navigationService, () => new SettingsViewModel());
        NavigateToApplicationLogCommand = new NavigateCommand<ApplicationLogViewModel>(navigationService, () => new ApplicationLogViewModel());
        NavigateToAboutCommand = new NavigateCommand<AboutViewModel>(navigationService, () => new AboutViewModel());


        navigationStore[nameof(MainViewModel)].CurrentViewModelChanged += MainWindowViewModel_CurrentViewModelChanged;

        SaveApplicationStateCommand = new RelayCommand(SaveApplicationState, true);
        OpenAccountOverviewCommand = new RelayCommand<Window>(OpenAccountOverview, true);

        NavigateToExplorerCommand.Execute(NavigateCommandParameter);

        Application.Current.Dispatcher.InvokeAsync(() => {
            pluginManager.LoadAssemblies();
        });
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
}
