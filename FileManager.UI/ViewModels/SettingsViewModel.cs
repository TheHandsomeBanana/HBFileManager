﻿using FileManager.UI.Models.SettingsModels;
using FileManager.UI.Services.SettingsService;
using FileManager.UI.ViewModels.SettingsViewModels;
using HBLibrary.DI;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.ViewModels;
using Unity;

namespace FileManager.UI.ViewModels;
public class SettingsViewModel : ViewModelBase, IDisposable {
    private readonly INavigationStore navigationStore;
    private readonly ISettingsService settingsService;
    public ViewModelBase? CurrentViewModel => navigationStore[NavigateCommandParameter].ViewModel;
    public string NavigateCommandParameter => nameof(SettingsViewModel);


    public NavigateCommand<SettingsEnvironmentViewModel> NavigateToEnvironmentCommand { get; set; }
    public NavigateCommand<SettingsExecutionViewModel> NavigateToExecutionCommand { get; set; }
    public NavigateCommand<SettingsWinRARViewModel> NavigateToWinRARCommand { get; set; }
    public NavigateCommand<SettingsPluginsViewModel> NavigateToPluginsCommand { get; set; }

    public SettingsViewModel() {
        IUnityContainer container = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid)!;
        INavigationService navigationService = container.Resolve<INavigationService>();

        settingsService = container.Resolve<ISettingsService>();

        SettingsEnvironmentModel environmentModel = settingsService.GetOrSetNew(() => new SettingsEnvironmentModel());
        SettingsWinRARModel winRARModel = settingsService.GetOrSetNew(() => new SettingsWinRARModel());
        SettingsExecutionModel executionModel = settingsService.GetOrSetNew(() => new SettingsExecutionModel());

        NavigateToEnvironmentCommand = new NavigateCommand<SettingsEnvironmentViewModel>(navigationService, () => new SettingsEnvironmentViewModel(environmentModel));
        NavigateToExecutionCommand = new NavigateCommand<SettingsExecutionViewModel>(navigationService, () => new SettingsExecutionViewModel(executionModel));
        NavigateToWinRARCommand = new NavigateCommand<SettingsWinRARViewModel>(navigationService, () => new SettingsWinRARViewModel(winRARModel));
        NavigateToPluginsCommand = new NavigateCommand<SettingsPluginsViewModel>(navigationService, () => new SettingsPluginsViewModel());

        NavigateToEnvironmentCommand.Execute(NavigateCommandParameter);

        this.navigationStore = container.Resolve<INavigationStore>();
        navigationStore[NavigateCommandParameter].CurrentViewModelChanged += SettingsViewModel_CurrentViewModelChanged;
    }

    private void SettingsViewModel_CurrentViewModelChanged() {
        NotifyPropertyChanged(nameof(CurrentViewModel));
    }

    public void Dispose() {
        navigationStore[NavigateCommandParameter].CurrentViewModelChanged -= SettingsViewModel_CurrentViewModelChanged;
        navigationStore.DisposeByParentTypename(NavigateCommandParameter);
    }
}
