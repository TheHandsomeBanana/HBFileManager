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
    /// Interaction logic for JobsHistoryView.xaml
    /// </summary>
    public partial class JobsHistoryView : UserControl
    {
        public JobsHistoryView()
        {
            InitializeComponent();
            Loaded += JobsHistoryView_Loaded;
        }

        private void JobsHistoryView_Loaded(object sender, RoutedEventArgs e) {
            if (DataContext is JobsHistoryViewModel viewModel && !viewModel.IsInitialized) {
                Dispatcher.Invoke(viewModel.InitializeAsync);
            }
        }
    }
}
