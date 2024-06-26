using FileManager.UI.ViewModels.SettingsViewModels;
using FileManager.UI.Views.SettingsViews;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
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
    private readonly IViewModelCache viewModelCache;
    private ViewModelBase currentViewModel;
    public ViewModelBase CurrentViewModel {
        get => currentViewModel;
        set {
            currentViewModel = value;
            NotifyPropertyChanged();
        }
    }

    public RelayCommand ChangeViewCommand { get; set; }

    public ObservableCollection<TreeViewItem> TreeViewItems { get; set; }

    public SettingsViewModel() {
        ChangeViewCommand = new RelayCommand(ChangeView, true);

        TreeViewItems = [
            new TreeViewItem("Environment", typeof(SettingsEnvironmentViewModel)),
            new TreeViewItem("Execution", typeof(SettingsExecutionViewModel)),
            new TreeViewItem("WinRAR", typeof(SettingsWinRARViewModel)),
        ];

        viewModelCache = UnityBase.GetChildContainer(nameof(FileManager)).Resolve<IViewModelCache>();
        currentViewModel = viewModelCache.GetOrNew<SettingsEnvironmentViewModel>();
    }

    private void ChangeView(object obj) {
        TreeViewItem item = (TreeViewItem)obj;
        viewModelCache.AddOrUpdate(CurrentViewModel);
        CurrentViewModel = viewModelCache.GetOrNew(item.ViewModelType);
    }
}
