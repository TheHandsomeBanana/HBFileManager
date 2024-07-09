using FileManager.UI.Models.SettingsPageModels;
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
    private readonly INavigationService navigationService;
    private readonly INavigationStore navigationStore;
    private readonly ISettingsService settingsService;
    public ViewModelBase CurrentViewModel => navigationStore[nameof(SettingsViewModel)].ViewModel;
    public RelayCommand ChangeViewCommand { get; set; }

    public ObservableCollection<TreeViewItem> TreeViewItems { get; set; }

    public SettingsViewModel() {
        ChangeViewCommand = new RelayCommand(ChangeView, true);
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;

        this.navigationService = container.Resolve<INavigationService>();
        this.navigationStore = container.Resolve<INavigationStore>();
        navigationStore[nameof(SettingsViewModel)].CurrentViewModelChanged += SettingsViewModel_CurrentViewModelChanged;
        settingsService = container.Resolve<ISettingsService>();

        SettingsWinRARModel winRARModel = settingsService.GetOrSetNew(() => new SettingsWinRARModel())!;

        TreeViewItems = [
            new TreeViewItem("Environment", new SettingsEnvironmentViewModel()),
            new TreeViewItem("Execution", new SettingsExecutionViewModel()),
            new TreeViewItem("WinRAR", new SettingsWinRARViewModel(winRARModel!)),
        ];
    }

    private void SettingsViewModel_CurrentViewModelChanged() {
        NotifyPropertyChanged(nameof(CurrentViewModel));
    }

    private void ChangeView(object? obj) {
        if (obj is TreeViewItem treeViewItem) {
            navigationService.Navigate(nameof(SettingsViewModel), treeViewItem.ViewModel);
        }
    }
}
