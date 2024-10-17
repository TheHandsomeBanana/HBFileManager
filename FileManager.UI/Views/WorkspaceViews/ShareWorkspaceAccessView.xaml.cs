using FileManager.UI.ViewModels.WorkspaceViewModels;
using HBLibrary.Common.Extensions;
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

namespace FileManager.UI.Views.WorkspaceViews;
/// <summary>
/// Interaction logic for ShareWorkspaceAccessView.xaml
/// </summary>
public partial class ShareWorkspaceAccessView : UserControl {
    public ShareWorkspaceAccessView() {
        Loaded += ShareWorkspaceAccessView_Loaded;
        InitializeComponent();
    }

    private void ShareWorkspaceAccessView_Loaded(object sender, RoutedEventArgs e) {
        if (DataContext is ShareWorkspaceAccessViewModel shareWorkspaceAccessViewModel) {
            this.Dispatcher.Invoke(shareWorkspaceAccessViewModel.InitializeAsync);
        }
    }
}
