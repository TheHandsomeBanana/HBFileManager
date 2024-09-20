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

        SettingsEnvironmentModel? environmentSettings = settingsService.GetSetting<SettingsEnvironmentModel>();
        if(environmentSettings is not null && environmentSettings!.PreloadPluginAssemblies) {

            Application.Current.Dispatcher.InvokeAsync(() => {
                IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
                IPluginManager pluginManager = container.Resolve<IPluginManager>();
                pluginManager.LoadAssemblies();
            });

        }
    }

    private void OpenAccountOverview(Window obj) {
        AccountViewModel accountViewModel = new AccountViewModel(obj, 
            accountService, 
            commonAppSettings, s => UserSwitchCallback(obj, s), 
            ((App)Application.Current).PreventShutdown
        );

        HBDarkAccountWindow accountWindow = new HBDarkAccountWindow(obj, accountViewModel);
        accountWindow.Show();
    }

    private void SaveApplicationState(object? obj) {
        App.SaveApplicationState();
        HBDarkMessageBox.Show("Saved", "Application state saved successfully.");
    }

    private void MainWindowViewModel_CurrentViewModelChanged() {
        NotifyPropertyChanged(nameof(CurrentViewModel));
    }

    private void UserSwitchCallback(Window obj, bool success) {
        if (success) {
            // User changed
            // -> Use new storage containers
            applicationStorage.RemoveAllContainers();
            App.AddApplicationStorageContainers(applicationStorage, accountService);

            obj = new MainWindow {
                DataContext = new MainViewModel()
            };

            obj.Closed += (_, _) => {
                App currentApp = (App)Application.Current;
                if (currentApp.CanShutdown) {
                    Application.Current.Shutdown();
                }
                else {
                    currentApp.AllowShutdown();
                }
            };

            // Change PluginManager folder based on logged in user
            IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
            IPluginManager pluginManager = container.Resolve<IPluginManager>();
            pluginManager.SwitchContext(App.GetPluginStoragePath(container), true, false);

            obj.Show();
        }
        else {
            Application.Current.Shutdown();
        }
    }
}
