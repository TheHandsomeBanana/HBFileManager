using FileManager.UI.ViewModels;
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

namespace FileManager.UI.Views {
    /// <summary>
    /// Interaction logic for ScriptingPage.xaml
    /// </summary>
    public partial class ScriptingPage : Page {
        public ScriptingPage() {
            InitializeComponent();

            IViewModelCache viewModelCache = UnityBase.GetChildContainer(nameof(FileManager)).Resolve<IViewModelCache>();
            ScriptingPageViewModel viewModel = viewModelCache.GetOrNew<ScriptingPageViewModel>();
            viewModelCache.AddOrUpdate(viewModel);

            this.DataContext = viewModel;
        }
    }
}
