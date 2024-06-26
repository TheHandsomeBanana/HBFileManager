using HBLibrary.Common.DI.Unity;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Wpf.Extensions;
using HBLibrary.Wpf.Navigation;
using HBLibrary.Wpf.Services;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using Unity;
using Unity.Lifetime;
using FileManager.UI.ViewModels;
using FileManager.UI.ViewModels.SettingsViewModels;

namespace FileManager.UI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static readonly Uri BaseUri = new Uri("pack://application:,,,/");
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            IUnityContainer container = UnityBase.CreateChildContainer(nameof(FileManager));
            container.RegisterSingleton<IViewModelCache, ViewModelCache>();


            string storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage");
            Directory.CreateDirectory(storagePath);
            container.RegisterInstance<IApplicationStorage>(new ApplicationStorage(storagePath));

        }

        
    }
}
