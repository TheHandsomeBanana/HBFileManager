using FileManager.UI.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FileManager.UI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            StateChanged += MainWindowStateChangeRaised;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            if(DataContext is MainViewModel mainViewModel) {
                Dispatcher.InvokeAsync(mainViewModel.InitializeAsync);
            }
        }

        public MainWindow(ApplicationState appState) : this() {
            WindowState = appState.WindowState;

            if (appState.Left is not null) {
                Left = appState.Left.Value;
            }

            if (appState.Top is not null) {
                Top = appState.Top!.Value;
            }


            if (WindowState == WindowState.Maximized) {
                SetMaximizedState();
            }
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
                SetMaximizedState();
            }
            else {
                MainBorder.Margin = new Thickness(0);
                MainBorder.BorderThickness = new Thickness(1);
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }


        private void SetMaximizedState() {
            // Margin = WindowChrome ResizeBorder and CaptionHeight
            MainBorder.Margin = new Thickness(5, 5, 5, 45);
            MainBorder.BorderThickness = new Thickness(0);
            RestoreButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Collapsed;
        }
    }
}