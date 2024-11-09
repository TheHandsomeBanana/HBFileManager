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

    private void JobsView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) {
        if (DataContext is JobsViewModel newJobsViewModel && !newJobsViewModel.IsInitialized) {
            this.Dispatcher.InvokeAsync(newJobsViewModel.Initialize);
        }

        //if(e.OldValue is IDisposable disposable) {
        //    disposable.Dispose();
        //}
    }
}
