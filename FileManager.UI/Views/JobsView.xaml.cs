using FileManager.UI.ViewModels;
using System.Windows.Controls;

namespace FileManager.UI.Views;
/// <summary>
/// Interaction logic for JobsView.xaml
/// </summary>
public partial class JobsView : UserControl {
    public JobsView() {
        this.DataContextChanged += JobsView_DataContextChanged;
        InitializeComponent();
    }

    // Is only called 2 times
    // When entering -> Init
    // When leaving -> Dispose
    private void JobsView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) {
        if (e.OldValue is JobsViewModel jobsViewModel) {
            jobsViewModel.Dispose();
        }

        if (e.NewValue is JobsViewModel newJobsViewModel) {
            this.Dispatcher.InvokeAsync(() => {
                newJobsViewModel.Initialize();
            });
        }
    }
}
