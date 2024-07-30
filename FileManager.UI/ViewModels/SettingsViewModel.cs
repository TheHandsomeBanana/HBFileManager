using FileManager.UI.Models.SettingsModels;
using FileManager.UI.Services.SettingsService;
using FileManager.UI.ViewModels.SettingsViewModels;
using FileManager.UI.Views.SettingsViews;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.Services.NavigationService.Single;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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

    public SettingsViewModel() {
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
        INavigationService navigationService = container.Resolve<INavigationService>();

        this.navigationStore = container.Resolve<INavigationStore>();
        navigationStore[nameof(SettingsViewModel)].CurrentViewModelChanged += SettingsViewModel_CurrentViewModelChanged;
        settingsService = container.Resolve<ISettingsService>();

        SettingsEnvironmentModel environmentModel = settingsService.GetOrSetNew(() => new SettingsEnvironmentModel());
        SettingsWinRARModel winRARModel = settingsService.GetOrSetNew(() => new SettingsWinRARModel());

        NavigateToEnvironmentCommand = new NavigateCommand<SettingsEnvironmentViewModel>(navigationService, () => new SettingsEnvironmentViewModel(environmentModel));
        NavigateToExecutionCommand = new NavigateCommand<SettingsExecutionViewModel>(navigationService, () => new SettingsExecutionViewModel());
        NavigateToWinRARCommand = new NavigateCommand<SettingsWinRARViewModel>(navigationService, () => new SettingsWinRARViewModel(winRARModel));

        NavigateToEnvironmentCommand.Execute(NavigateCommandParameter);
    }

    private void SettingsViewModel_CurrentViewModelChanged() {
        NotifyPropertyChanged(nameof(CurrentViewModel));
    }
}
