using FileManager.UI.Models;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Extensions;
using HBLibrary.Wpf.Navigation;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
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
public class MainWindowViewModel : ViewModelBase {
    private readonly IViewModelCache viewModelCache;
    private ViewModelBase currentViewModel;
    public ViewModelBase CurrentViewModel {
        get => currentViewModel;
        set {
            currentViewModel = value;
            NotifyPropertyChanged();
        }
    }

    public RelayCommand NavigateToExplorer { get; set; }
    public RelayCommand NavigateToScripting { get; set; }
    public RelayCommand NavigateToExecution { get; set; }
    public RelayCommand NavigateToSettings { get; set; }
    public RelayCommand NavigateToApplicationLog { get; set; }
    public RelayCommand NavigateToAbout { get; set; }


    public RelayCommand WindowClosedCommand { get; set; }





    public MainWindowViewModel() {
        IUnityContainer? container = UnityBase.GetChildContainer(nameof(FileManager));
        if (container is not null) {
            viewModelCache = container.Resolve<IViewModelCache>();
            CurrentViewModel = viewModelCache.GetOrNew<SettingsViewModel>();
        }

        WindowClosedCommand = new RelayCommand(OnWindowClosed, true);

        NavigateToExplorer = new RelayCommand(Navigate<ExplorerViewModel>, true);
        NavigateToScripting = new RelayCommand(Navigate<ScriptingViewModel>, true);
        NavigateToExecution = new RelayCommand(Navigate<ExecutionViewModel>, true);
        NavigateToSettings = new RelayCommand(Navigate<SettingsViewModel>, true);
        NavigateToApplicationLog = new RelayCommand(Navigate<ApplicationLogViewModel>, true);
        NavigateToAbout = new RelayCommand(Navigate<AboutViewModel>, true);

    }


    private void Navigate<TViewModel>(object o) where TViewModel : ViewModelBase, new() {
        viewModelCache.AddOrUpdate(CurrentViewModel);
        CurrentViewModel = viewModelCache.GetOrNew<TViewModel>();
    }

    private void OnWindowClosed(object obj) {

    }
}
