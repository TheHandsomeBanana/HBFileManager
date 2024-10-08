using FileManager.UI.ViewModels;
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

namespace FileManager.UI.Views;
/// <summary>
/// Interaction logic for WorkspacesView.xaml
/// </summary>
public partial class WorkspacesView : UserControl {
    public WorkspacesView() {
        Loaded += WorkspacesView_Loaded;
        InitializeComponent();
    }

    private void WorkspacesView_Loaded(object sender, RoutedEventArgs e) {
        if (DataContext is WorkspacesViewModel workspacesViewModel) {
            Dispatcher.InvokeAsync(async () => {
                await workspacesViewModel.InitializeAsync();
            });
        }
    }
}
