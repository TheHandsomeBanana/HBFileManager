using FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
using FileManager.UI.ViewModels.JobViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileManager.UI.Views.ExecutionViews.RunningJobsViews;
/// <summary>
/// Interaction logic for RunningJobView.xaml
/// </summary>
public partial class RunningJobView : UserControl {
    public RunningJobView() {
        DataContextChanged += RunningJobView_DataContextChanged;
        InitializeComponent();
    }

    private void RunningJobView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
        if (DataContext is RunningJobViewModel viewModel && !viewModel.IsInitialized) {
            Dispatcher.Invoke(viewModel.Initialize);
        }
    }
}
