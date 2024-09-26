using HBLibrary.Common.Initializer;
using System.Windows.Controls;

namespace FileManager.UI.Views.JobViews;
/// <summary>
/// Interaction logic for JobItemView.xaml
/// </summary>
public partial class JobItemView : UserControl {
    public JobItemView() {
        InitializeComponent();
        this.Loaded += JobItemView_Loaded;
        this.Unloaded += JobItemView_Unloaded;
    }

    private async void JobItemView_Loaded(object sender, System.Windows.RoutedEventArgs e) {
        if (DataContext is IAsyncInitializer initializer) {
            await initializer.InitializeAsync();
        }
    }

    private void JobItemView_Unloaded(object sender, System.Windows.RoutedEventArgs e) {
        if(DataContext is IDisposable disposableDataContext) {
            disposableDataContext.Dispose();
        }
    }
}
