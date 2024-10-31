using FileManager.UI.ViewModels.ExecutionViewModels;
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

namespace FileManager.UI.Views.ExecutionViews
{
    /// <summary>
    /// Interaction logic for RunningJobsView.xaml
    /// </summary>
    public partial class RunningJobsView : UserControl
    {
        public RunningJobsView()
        {
            InitializeComponent();
            Loaded += RunningJobsView_Loaded;
        }

        private void RunningJobsView_Loaded(object sender, RoutedEventArgs e) {
            if (DataContext is RunningJobsViewModel viewModel && !viewModel.IsInitialized) {
                viewModel.Initialize();
            }
        }
    }
}
