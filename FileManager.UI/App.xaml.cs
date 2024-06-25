using HBLibrary.Common.DI.Unity;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Wpf.Extensions;
using HBLibrary.Wpf.Navigation;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.Services.FrameNavigationService;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using Unity;
using Unity.Lifetime;
using FileManager.UI.ViewModels;
using FileManager.UI.ViewModels.SettingsPageViewModels;

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

            //container.RegisterInstance<IFrameNavigationService>(GetNavigationService(container.Resolve<IViewModelCache>()));
        }

        private FrameNavigationService GetNavigationService(IViewModelCache viewModelCache) {
            FrameNavigationService navigationService = new FrameNavigationService(viewModelCache);
            navigationService.RegisterFrameNavigation(b => b
                 .Add("MainWindowFrame", mb =>
                    mb.AddBasePath(BaseUri.OriginalString)
                    .Add(new Uri(BaseUri, "FileManager.UI;component/Views/ExplorerPage.xaml"), typeof(ExplorerPageViewModel))
                    .Add(new Uri(BaseUri, "FileManager.UI;component/Views/ScriptingPage.xaml"), typeof(ScriptingPageViewModel))
                    .Add(new Uri(BaseUri, "FileManager.UI;component/Views/ExecutionPage.xaml"), typeof(ExecutionPageViewModel))
                    .Add(new Uri(BaseUri, "FileManager.UI;component/Views/SettingsPage.xaml"), typeof(SettingsPageViewModel))
                    .Add(new Uri(BaseUri, "FileManager.UI;component/Views/ApplicationLogPage.xaml"), typeof(ApplicationLogPageViewModel))
                    .Add(new Uri(BaseUri, "FileManager.UI;component/Views/AboutPage.xaml"), typeof(AboutPageViewModel))
                    .Build())
                .Add("SettingsPageFrame", mb =>
                    mb.AddBasePath(BaseUri.OriginalString)
                    .Add(new Uri(BaseUri, "FileManager.UI;component/Views/SettingsPageViews/SettingsEnvironmentPage.xaml"), typeof(SettingsEnvironmentPageViewModel))
                    .Add(new Uri(BaseUri, "FileManager.UI;component/Views/SettingsPageViews/SettingsExecutionPage.xaml"), typeof(SettingsExecutionPageViewModel))
                    .Add(new Uri(BaseUri, "FileManager.UI;component/Views/SettingsPageViews/SettingsWinRARPage.xaml"), typeof(SettingsWinRARPageViewModel))
                    .Build())
                .Build());

            return navigationService;
        }
    }
}
