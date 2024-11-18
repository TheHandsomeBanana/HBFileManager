using FileManager.UI.ViewModels.JobViewModels;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FileManager.UI.Views.JobViews;
/// <summary>
/// Interaction logic for JobItemView.xaml
/// </summary>
public partial class JobItemView : UserControl {
    public JobItemView() {
        this.DataContextChanged += JobItemView_DataContextChanged;
        InitializeComponent();
    }

    private async void JobItemView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) {
        if (e.NewValue is JobItemViewModel jobItemViewModel && !jobItemViewModel.IsInitialized) {
            await this.Dispatcher.Invoke(jobItemViewModel.InitializeAsync);
        };

        if(e.OldValue is JobItemViewModel oldJobItemViewModel) {
            oldJobItemViewModel.Reset();
        }
    }
}
