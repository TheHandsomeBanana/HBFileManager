using HBLibrary.Common.DI.Unity;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Wpf.Services;
using System.IO;
using System.Windows;
using Unity;
using Unity.Lifetime;

namespace FileManager.UI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            string storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage");
            Directory.CreateDirectory(storagePath);

            IUnityContainer container = UnityBase.CreateChildContainer(nameof(FileManager));
            container.RegisterSingleton<IViewModelCache, ViewModelCache>();
            container.RegisterSingleton<IFrameNavigationService, FrameNavigationService>();

            container.RegisterInstance<IApplicationStorage>(new ApplicationStorage(storagePath));
        }
    }

}
