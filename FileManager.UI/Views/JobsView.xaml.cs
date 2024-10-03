using FileManager.UI.ViewModels;
using System.Windows.Controls;

namespace FileManager.UI.Views;
/// <summary>
/// Interaction logic for JobsView.xaml
/// </summary>
public partial class JobsView : UserControl {
    public JobsView() {
        this.Loaded += JobsView_Loaded; ;
        InitializeComponent();
    }

    // Disposal is handled by NavigationService
    private void JobsView_Loaded(object sender, System.Windows.RoutedEventArgs e) {
        if (DataContext is JobsViewModel newJobsViewModel) {
            this.Dispatcher.InvokeAsync(() => {
                newJobsViewModel.Initialize();
            });
        }
    }
}
