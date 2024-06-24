using FileManager.UI.ViewModels;
using FileManager.UI.ViewModels.SettingsPageViewModels;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Services;
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
using Unity;

namespace FileManager.UI.Views;
/// <summary>
/// Interaction logic for AboutPage.xaml
/// </summary>
public partial class AboutPage : Page {
    public AboutPage() {
        InitializeComponent();

        IViewModelCache viewModelCache = UnityBase.GetChildContainer(nameof(FileManager)).Resolve<IViewModelCache>();
        AboutPageViewModel viewModel = viewModelCache.GetOrNew<AboutPageViewModel>();
        viewModelCache.AddOrUpdate(viewModel);

        this.DataContext = viewModel;
    }
}
