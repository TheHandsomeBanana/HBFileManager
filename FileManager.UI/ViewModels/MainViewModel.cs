using FileManager.UI.Models;
using FileManager.UI.Models.SettingsPageModels;
using FileManager.UI.ViewModels.SettingsViewModels;
using HBLibrary.Common;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Services.IO.Storage.Entries;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Extensions;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.Services.NavigationService.Single;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup.Localizer;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Unity;

namespace FileManager.UI.ViewModels;
public class MainViewModel : ViewModelBase {
    private readonly INavigationStore navigationStore;
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

    public MainViewModel() {
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;


        this.navigationStore = container.Resolve<INavigationStore>();
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
    }

    private void SaveApplicationState(object? obj) {
        App.SaveApplicationState();
        HBDarkMessageBox.Show("Saved", "Application state saved successfully.");
    }

    private void MainWindowViewModel_CurrentViewModelChanged() {
        NotifyPropertyChanged(nameof(CurrentViewModel));
    }
}
