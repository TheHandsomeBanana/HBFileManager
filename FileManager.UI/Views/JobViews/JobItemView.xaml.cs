using FileManager.UI.ViewModels.JobViewModels;
using System.Windows.Controls;

namespace FileManager.UI.Views.JobViews;
/// <summary>
/// Interaction logic for JobItemView.xaml
/// </summary>
public partial class JobItemView : UserControl {
    public JobItemView() {
        this.DataContextChanged += JobItemView_DataContextChanged;
        InitializeComponent();
    }

    private void JobItemView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) {
        if (e.NewValue is JobItemViewModel jobItemViewModel && !jobItemViewModel.IsInitialized) {
            this.Dispatcher.InvokeAsync(async () => {
                await jobItemViewModel.InitializeAsync();
            });
        };
    }
}
