using HBLibrary.Common.DI.Unity;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Wpf.Extensions;
using HBLibrary.Wpf.Services;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using Unity;
using Unity.Lifetime;
using FileManager.UI.ViewModels;
using FileManager.UI.ViewModels.SettingsViewModels;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.Services.NavigationService.Single;
using HBLibrary.Wpf.ViewModels;
using System.Diagnostics;
using FileManager.Core.Models;
using HBLibrary.Services.IO.Storage.Entries;
using HBLibrary.Common;
using FileManager.UI.Models.SettingsPageModels;
using HBLibrary.Services.IO.Json;

namespace FileManager.UI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        public App() {
            this.Exit += (_, _) => SaveApplicationState();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => SaveApplicationState();
            AppDomain.CurrentDomain.UnhandledException += (_, _) => SaveApplicationState();
            

            IUnityContainer container = UnityBase.CreateChildContainer(nameof(FileManager));
            container.RegisterSingleton<INavigationService, NavigationService>();
            container.RegisterSingleton<INavigationStore, NavigationStore>();
            container.RegisterFactory<IJsonFileService>(s => {
                JsonFileService jsonFileService = new JsonFileService();
                jsonFileService.UseBase64 = true;
                return jsonFileService;
            }, new ContainerControlledLifetimeManager());

            string storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage");
            Directory.CreateDirectory(storagePath);
            ApplicationStorage appStorage = new ApplicationStorage();
            appStorage.JsonFileService = container.Resolve<IJsonFileService>();
            appStorage.BasePath = storagePath;

            container.RegisterInstance<IApplicationStorage>(appStorage);

            InitializeNavigationStore(container);
        }

        

        private void InitializeNavigationStore(IUnityContainer container) {
            ExplorerViewModel mainViewModelStartControl = new ExplorerViewModel();
            SettingsEnvironmentViewModel settingsViewModelStartControl = new SettingsEnvironmentViewModel();

            IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();
            
            IStorageEntry<SettingsWinRARCoreModel> settingsWinRARCoreModelEntry = applicationStorage.GetStorageEntry<SettingsWinRARCoreModel>(StorageEntryType.Json, false);

            SettingsWinRARCoreModel settingsWinRARCoreModel = settingsWinRARCoreModelEntry.Get() ?? new SettingsWinRARCoreModel();
            SettingsWinRARModel settingsWinRARModel = AutoMapper.MapUnsafe<SettingsWinRARCoreModel, SettingsWinRARModel>(settingsWinRARCoreModel);

            var navigationStore = container.Resolve<INavigationStore>();
            navigationStore[nameof(MainViewModel)] = new ActiveViewModel(mainViewModelStartControl);
            navigationStore[nameof(SettingsViewModel)] = new ActiveViewModel(settingsViewModelStartControl);

            navigationStore.InitViewModelInstances(e =>
                e.AddViewModel(mainViewModelStartControl)
                .AddViewModel(new ScriptingViewModel())
                .AddViewModel(new ExecutionViewModel())
                .AddViewModel(new SettingsViewModel())
                .AddViewModel(new ApplicationLogViewModel())
                .AddViewModel(new AboutViewModel())
                .AddViewModel(settingsViewModelStartControl)
                .AddViewModel(new SettingsExecutionViewModel())
                .AddViewModel(new SettingsWinRARViewModel(settingsWinRARModel))
                .Build());
        }

        public static void SaveApplicationState() {
            IUnityContainer? container = UnityBase.GetChildContainer(nameof(FileManager));
            if (container is null)
                return;

            IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();
            INavigationStore navigationStore = container.Resolve<INavigationStore>();

            SettingsWinRARViewModel settingsWinRARViewModel = navigationStore.GetStoredViewModel<SettingsWinRARViewModel>();
            SettingsWinRARCoreModel settingsWinRARCoreModel = AutoMapper.MapUnsafe<SettingsWinRARModel, SettingsWinRARCoreModel>(settingsWinRARViewModel.Model);


            applicationStorage.SaveStorageEntry(settingsWinRARCoreModel, StorageEntryType.Json);
        }
    }
}
