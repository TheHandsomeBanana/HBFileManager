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
    public ViewModelBase CurrentViewModel => navigationStore[nameof(SettingsViewModel)].ViewModel;
    public RelayCommand ChangeViewCommand { get; set; }

    public ObservableCollection<TreeViewItem> TreeViewItems { get; set; }

    public SettingsViewModel() {
        ChangeViewCommand = new RelayCommand(ChangeView, true);

        TreeViewItems = [
            new TreeViewItem("Environment", typeof(SettingsEnvironmentViewModel)),
            new TreeViewItem("Execution", typeof(SettingsExecutionViewModel)),
            new TreeViewItem("WinRAR", typeof(SettingsWinRARViewModel)),
        ];

        IUnityContainer? container = UnityBase.GetChildContainer(nameof(FileManager));

        if (container is not null) {
            this.navigationStore = container.Resolve<INavigationStore>();
            this.navigationService = container.Resolve<INavigationService>();
            navigationStore[nameof(SettingsViewModel)].CurrentViewModelChanged += SettingsViewModel_CurrentViewModelChanged;
        }
    }

    private void SettingsViewModel_CurrentViewModelChanged() {
        NotifyPropertyChanged(nameof(CurrentViewModel));
    }

    private void ChangeView(object? obj) {
        if (obj is TreeViewItem treeViewItem) {
            navigationService.Navigate(nameof(SettingsViewModel), treeViewItem.ViewModelType);
        }
    }
}
