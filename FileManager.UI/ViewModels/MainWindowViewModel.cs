using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Extensions;
using HBLibrary.Wpf.Navigation;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup.Localizer;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FileManager.UI.ViewModels;
public class MainWindowViewModel : ViewModelBase {
    public RelayCommand WindowClosedCommand { get; set; }
    public ObservableCollection<NavButton> NavButtons { get; set; }


    private NavButton currentPageSelection;
    public NavButton CurrentPageSelection {
        get {
            return currentPageSelection;
        }
        set {
            currentPageSelection = value;
            NotifyPropertyChanged();

            if (currentPageSelection != null) {
                CurrentPageSource = currentPageSelection.NavLink;
            }
        }
    }

    private Uri currentPageSource;
    public Uri CurrentPageSource {
        get { return currentPageSource; }
        set {
            currentPageSource = value;
            NotifyPropertyChanged();
        }
    }

    public MainWindowViewModel() {
        WindowClosedCommand = new RelayCommand(OnWindowClosed, true);


        double height = 80;
        double iconHeight = 25.0;
        double iconWidth = 25.0;
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Stretch;
        SolidColorBrush foreground = Brushes.White;
        SolidColorBrush background = Brushes.Transparent;
        SolidColorBrush selectedBackground = BrushHelper.GetColorFromHex("#3d3d3d")!;


        NavButtons = [
            new NavButton { Text = "Explorer",
                Icon = Geometry.Parse("M10,4H4C2.89,4 2,4.89 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V8C22,6.89 21.1,6 20,6H12L10,4Z"),
                IconHeight = iconHeight,
                IconWidth = iconWidth,
                NavLink = new Uri("Views/ExplorerPage.xaml", UriKind.Relative),
                Height = height,
                Foreground = foreground,
                HorizontalAlignment = horizontalAlignment,
                SelectedBackground = selectedBackground,
                Background = background,
                IconFill = BrushHelper.GetColorFromHex("#f1c23c")!
            },
            new NavButton { Text = "Scripting",
                Icon = Geometry.Parse("M14.6,16.6L19.2,12L14.6,7.4L16,6L22,12L16,18L14.6,16.6M9.4,16.6L4.8,12L9.4,7.4L8,6L2,12L8,18L9.4,16.6Z"),
                IconHeight = iconHeight,
                IconWidth = iconWidth,
                IconFill = BrushHelper.GetColorFromHex("#5DADE2")!,
                NavLink = new Uri("Views/ScriptingPage.xaml", UriKind.Relative),
                Height = height,
                Foreground = foreground,
                HorizontalAlignment = horizontalAlignment,
                SelectedBackground = selectedBackground,
                Background = background,
            },
            new NavButton { Text = "Execution",
                Icon = Geometry.Parse("M8.5,8.64L13.77,12L8.5,15.36V8.64M6.5,5V19L17.5,12"),
                IconHeight = iconHeight,
                IconWidth = iconWidth,
                IconFill = BrushHelper.GetColorFromHex("#8ae28a")!,
                NavLink = new Uri("Views/ExecutionPage.xaml", UriKind.Relative),
                Height = height,
                Foreground = foreground,
                HorizontalAlignment = horizontalAlignment,
                SelectedBackground = selectedBackground,
                Background = background,
            },
            new NavButton { Text = "Settings",
                Icon = Geometry.Parse("M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.21,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.21,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.67 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z"),
                IconHeight = iconHeight,
                IconWidth = iconWidth,
                IconFill = BrushHelper.GetColorFromHex("#cdd5e0")!,
                NavLink = new Uri("Views/SettingsPage.xaml", UriKind.Relative),
                Height = height,
                Foreground = foreground,
                HorizontalAlignment = horizontalAlignment,
                SelectedBackground = selectedBackground,
                Background = background,
            },
            new NavButton { Text = "About",
                Icon = Geometry.Parse("M11,9H13V7H11M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,17H13V11H11V17Z"),
                IconHeight = iconHeight,
                IconWidth = iconWidth,
                IconFill = BrushHelper.GetColorFromHex("#1d7ccc")!,
                NavLink = new Uri("Views/AboutPage.xaml", UriKind.Relative),
                Height = height,
                Foreground = foreground,
                HorizontalAlignment = horizontalAlignment,
                SelectedBackground = selectedBackground,
                Background = background,
            }
        ];

        currentPageSelection = NavButtons[0];
        currentPageSource = NavButtons[0].NavLink;
    }

    private void OnWindowClosed(object obj) {
        
    }
}
