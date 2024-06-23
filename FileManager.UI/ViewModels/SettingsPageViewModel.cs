using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels;
public class SettingsPageViewModel : ViewModelBase {
    private Uri currentPageSource;
    public Uri CurrentPageSource {
        get => currentPageSource;
        set {
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
        Options = [
            new TreeViewItem {
                Name = "Environment",
                NavLink = new Uri("SettingsPageViews/SettingsEnvironmentPage.xaml", UriKind.Relative),
                Children = [
                        new TreeViewItem { Name = "General", NavLink = new Uri("SettingsPageViews/SettingsEnvironmentPage.xaml", UriKind.Relative) },
                ],
            },
            new TreeViewItem { Name = "WinRAR", NavLink = new Uri("SettingsPageViews/SettingsWinRARPage.xaml", UriKind.Relative) },
        ];

        selectedOption = Options[0];
        currentPageSource = Options[0].NavLink!;
    }
}
