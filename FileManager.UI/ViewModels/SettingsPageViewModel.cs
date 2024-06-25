using FileManager.UI.ViewModels.SettingsPageViewModels;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace FileManager.UI.ViewModels;
public class SettingsPageViewModel : ViewModelBase {
    private readonly IViewModelCache viewModelCache;


    private Uri currentPageSource;
    public Uri CurrentPageSource {
        get => currentPageSource;
        set {
            ViewModelBase currentPageViewModel = GetViewModelForPage(currentPageSource);
            viewModelCache.AddOrUpdate(currentPageViewModel);

            currentPageSource = value;
            NotifyPropertyChanged();
        }
    }

    public ObservableCollection<TreeViewItem> Options { get; set; }

    private TreeViewItem selectedOption;
    public TreeViewItem SelectedOption {
        get => selectedOption;
        set {
            selectedOption = value;
            NotifyPropertyChanged();

            if (selectedOption is not null && selectedOption.NavLink is not null) {
                CurrentPageSource = selectedOption.NavLink;
            }
        }
    }

    public SettingsPageViewModel() {
        viewModelCache = UnityBase.GetChildContainer(nameof(FileManager)).Resolve<IViewModelCache>();

        Options = [
            new TreeViewItem {
                Name = "Environment",
                NavLink = new Uri("SettingsPageViews/SettingsEnvironmentPage.xaml", UriKind.Relative),
                Children = [
                        new TreeViewItem { Name = "General", NavLink = new Uri("SettingsPageViews/SettingsEnvironmentPage.xaml", UriKind.Relative) },
                ],
            },
            new TreeViewItem { Name = "Execution", NavLink = new Uri("SettingsPageViews/SettingsExecutionPage.xaml", UriKind.Relative) },
            new TreeViewItem { Name = "WinRAR", NavLink = new Uri("SettingsPageViews/SettingsWinRARPage.xaml", UriKind.Relative) },
        ];

        selectedOption = Options[0];
        currentPageSource = Options[0].NavLink!;
    }

    private ViewModelBase GetViewModelForPage(Uri value) {
        switch (value.ToString()) {
            case "SettingsPageViews/SettingsEnvironmentPage.xaml":
                return viewModelCache.GetOrNew<SettingsEnvironmentPageViewModel>();
            case "SettingsPageViews/SettingsExecutionPage.xaml":
                return viewModelCache.GetOrNew<SettingsExecutionPageViewModel>();
            case "SettingsPageViews/SettingsWinRARPage.xaml":
                return viewModelCache.GetOrNew<SettingsWinRARPageViewModel>();
        }

        throw new InvalidOperationException("Unknown page uri.");
    }
}
