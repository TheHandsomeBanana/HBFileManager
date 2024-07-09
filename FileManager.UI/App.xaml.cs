﻿using HBLibrary.Common.DI.Unity;
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
using HBLibrary.Common;
using FileManager.UI.Models.SettingsPageModels;
using HBLibrary.Services.IO.Json;
using FileManager.UI.Services;
using HBLibrary.Services.IO;
using HBLibrary.Services.IO.Storage.Builder;
using HBLibrary.Services.IO.Xml;
using HBLibrary.Common.Extensions;
using FileManager.UI.Services.SettingsService;

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

            INavigationStore navigationStore = new NavigationStore();
            navigationStore.AddDefaultViewModel(nameof(MainViewModel), new ExplorerViewModel());
            navigationStore.AddDefaultViewModel(nameof(SettingsViewModel), new SettingsEnvironmentViewModel());
            container.RegisterInstance(navigationStore);


            container.RegisterSingleton<INavigationService, NavigationService>();
            container.RegisterSingleton<IViewModelStore, ViewModelStore>();
            container.RegisterSingleton<IJobService, JobService>();
            container.RegisterType<IDialogService, DialogService>();


            AddApplicationStorage(container);

            container.RegisterType<ISettingsService, SettingsService>(new ContainerControlledLifetimeManager());
        }

        private void AddApplicationStorage(IUnityContainer container) {
            string storagePath = Path.Combine(GlobalEnvironment.ApplicationDataBasePath, "FileManager", "data");

            IApplicationStorageBuilder appStorageBuilder = ApplicationStorage.CreateBuilder(storagePath);
            appStorageBuilder.AddContainer(typeof(SettingsService), builder => {
                builder.SetContainerPath("settings");

                builder.ConfigureFileServices(fs => {
                    fs.UseJsonFileService(() => {
                        JsonFileService jsonFileService = new JsonFileService();
                        jsonFileService.UseBase64 = true;
                        return jsonFileService;
                    })
                    .UseFileService(() => new FileService())
                    .UseXmlFileService(() => new XmlFileService());
                });

                return builder.Build();
            });

            container.RegisterInstance(appStorageBuilder.Build(), InstanceLifetime.Singleton);
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
            applicationStorage.SaveAll();
        }
    }
}
