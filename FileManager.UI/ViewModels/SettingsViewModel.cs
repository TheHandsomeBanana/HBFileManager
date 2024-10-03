using FileManager.UI.Models.SettingsModels;
using FileManager.UI.Services.SettingsService;
using FileManager.UI.ViewModels.SettingsViewModels;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.ViewModels;
using Unity;

namespace FileManager.UI.ViewModels;
public class SettingsViewModel : ViewModelBase {
    private readonly INavigationStore navigationStore;
    private readonly ISettingsService settingsService;
    public ViewModelBase CurrentViewModel => navigationStore[nameof(SettingsViewModel)].ViewModel;
    public string NavigateCommandParameter => nameof(SettingsViewModel);


    public NavigateCommand<SettingsEnvironmentViewModel> NavigateToEnvironmentCommand { get; set; }
    public NavigateCommand<SettingsExecutionViewModel> NavigateToExecutionCommand { get; set; }
    public NavigateCommand<SettingsWinRARViewModel> NavigateToWinRARCommand { get; set; }
    public NavigateCommand<SettingsPluginsViewModel> NavigateToPluginsCommand { get; set; }

    public SettingsViewModel() {
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
        INavigationService navigationService = container.Resolve<INavigationService>();

        settingsService = container.Resolve<ISettingsService>();

        SettingsEnvironmentModel environmentModel = settingsService.GetOrSetNew(() => new SettingsEnvironmentModel());
        SettingsWinRARModel winRARModel = settingsService.GetOrSetNew(() => new SettingsWinRARModel());

        NavigateToEnvironmentCommand = new NavigateCommand<SettingsEnvironmentViewModel>(navigationService, () => new SettingsEnvironmentViewModel(environmentModel));
        NavigateToExecutionCommand = new NavigateCommand<SettingsExecutionViewModel>(navigationService, () => new SettingsExecutionViewModel());
        NavigateToWinRARCommand = new NavigateCommand<SettingsWinRARViewModel>(navigationService, () => new SettingsWinRARViewModel(winRARModel));
        NavigateToPluginsCommand = new NavigateCommand<SettingsPluginsViewModel>(navigationService, () => new SettingsPluginsViewModel());

        NavigateToEnvironmentCommand.Execute(NavigateCommandParameter);

        this.navigationStore = container.Resolve<INavigationStore>();
        navigationStore[nameof(SettingsViewModel)].CurrentViewModelChanged += SettingsViewModel_CurrentViewModelChanged;
    }

    private void SettingsViewModel_CurrentViewModelChanged() {
        NotifyPropertyChanged(nameof(CurrentViewModel));
    }
}
