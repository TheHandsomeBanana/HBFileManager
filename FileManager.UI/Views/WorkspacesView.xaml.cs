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
        DataContextChanged += WorkspacesView_DataContextChanged;
        InitializeComponent();
    }

    private void WorkspacesView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
        if (e.NewValue is WorkspacesViewModel workspacesViewModel && !workspacesViewModel.IsInitialized) {
            Dispatcher.Invoke(workspacesViewModel.InitializeAsync);
        }
    }
}
