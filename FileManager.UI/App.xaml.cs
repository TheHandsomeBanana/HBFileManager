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
        public static bool StateSaved { get; set; } = false;
        public App() {
            this.Exit += (_, _) => SaveApplicationState();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => SaveApplicationState();
            AppDomain.CurrentDomain.UnhandledException += (_, _) => SaveApplicationState();


            IUnityContainer container = UnityBase.CreateChildContainer(nameof(FileManager));

            container.RegisterSingleton<INavigationService, NavigationService>();
            container.RegisterSingleton<INavigationStore, NavigationStore>();
            container.RegisterSingleton<IViewModelStore, ViewModelStore>();

            container.RegisterFactory<IJsonFileService>(s => {
                JsonFileService jsonFileService = new JsonFileService();
                jsonFileService.UseBase64 = true;
                return jsonFileService;
            }, new ContainerControlledLifetimeManager());


            AddApplicationStorage(container);
            InitStores(container);
        }

        private void InitStores(IUnityContainer container) {
            IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();

            IStorageEntry<SettingsWinRARCoreModel> settingsWinRARCoreModelEntry = applicationStorage.GetStorageEntry<SettingsWinRARCoreModel>(StorageEntryType.Json, false);

            SettingsWinRARCoreModel settingsWinRARCoreModel = settingsWinRARCoreModelEntry.Get() ?? new SettingsWinRARCoreModel();
            SettingsWinRARModel settingsWinRARModel = AutoMapper.MapUnsafe<SettingsWinRARCoreModel, SettingsWinRARModel>(settingsWinRARCoreModel);

            INavigationStore navigationStore = container.Resolve<INavigationStore>();

            ExplorerViewModel mainViewModelEntryChild = new ExplorerViewModel();
            SettingsEnvironmentViewModel settingsViewModelEntryChild = new SettingsEnvironmentViewModel();

            navigationStore[nameof(MainViewModel)] = new ActiveViewModel(mainViewModelEntryChild);
            navigationStore[nameof(SettingsViewModel)] = new ActiveViewModel(settingsViewModelEntryChild);

            IViewModelStore viewModelStore = container.Resolve<IViewModelStore>();
            viewModelStore.InitViewModelInstances(e =>
                e.AddViewModel(new ExplorerViewModel())
                .AddViewModel(new JobsViewModel())
                .AddViewModel(new ScriptingViewModel())
                .AddViewModel(new ExecutionViewModel())
                .AddViewModel(new SettingsViewModel())
                .AddViewModel(new ApplicationLogViewModel())
                .AddViewModel(new AboutViewModel())
                .AddViewModel(new SettingsEnvironmentViewModel())
                .AddViewModel(new SettingsExecutionViewModel())
                .AddViewModel(new SettingsWinRARViewModel(settingsWinRARModel))
                .Build());

            
        }


        private void AddApplicationStorage(IUnityContainer container) {
            string storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage");
            Directory.CreateDirectory(storagePath);
            ApplicationStorage appStorage = new ApplicationStorage();
            appStorage.JsonFileService = container.Resolve<IJsonFileService>();
            appStorage.BasePath = storagePath;

            container.RegisterInstance<IApplicationStorage>(appStorage);
        }


        public static void SaveApplicationState() {
            if (StateSaved) {
                return;
            }

            StateSaved = true;

            IUnityContainer? container = UnityBase.GetChildContainer(nameof(FileManager));
            if (container is null)
                return;

            IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();
            IViewModelStore viewModelStore = container.Resolve<IViewModelStore>();

            SettingsWinRARViewModel settingsWinRARViewModel = viewModelStore.GetStoredViewModel<SettingsWinRARViewModel>();
            SettingsWinRARCoreModel settingsWinRARCoreModel = AutoMapper.MapUnsafe<SettingsWinRARModel, SettingsWinRARCoreModel>(settingsWinRARViewModel.Model);


            applicationStorage.SaveStorageEntry(settingsWinRARCoreModel, StorageEntryType.Json);
        }
    }
}
