using FileManager.UI.ViewModels;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Services;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;
using Unity;

namespace FileManager.UI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            StateChanged += MainWindowStateChangeRaised;

            //IFrameNavigationService navigationService = UnityBase.GetChildContainer(nameof(FileManager)).Resolve<IFrameNavigationService>();
            //navigationService.RegisterFrame("MainWindowFrame", MainWindowFrame);
            //MainWindowFrame.Source = new Uri(App.BaseUri, "FileManager.UI;component/Views/ExplorerPage.xaml");
        }

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e) {
            SystemCommands.MinimizeWindow(this);
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e) {
            SystemCommands.MaximizeWindow(this);
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e) {
            SystemCommands.RestoreWindow(this);
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e) {
            SystemCommands.CloseWindow(this);
        }

        // State change
        private void MainWindowStateChangeRaised(object? sender, EventArgs e) {
            if (WindowState == WindowState.Maximized) {
                MainBorder.Margin = new Thickness(8);
                MainBorder.BorderThickness = new Thickness(0);
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else {
                MainBorder.Margin = new Thickness(0);
                MainBorder.BorderThickness = new Thickness(1);
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }
    }
}