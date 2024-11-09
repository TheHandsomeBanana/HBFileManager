using FileManager.Core.Workspace;
using FileManager.UI.Services.SettingsService;
using FileManager.UI.ViewModels;
using HBLibrary.Wpf.Services.NavigationService.Builder;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Lifetime;
using System.Text.Json;
using HBLibrary.Common.Json;
using Microsoft.Identity.Client.Desktop;
using HBLibrary.Interface.DI;
using HBLibrary.Interface.Logging;
using HBLibrary.Logging;
using HBLibrary.Core;
using HBLibrary.Interface.Security.Account;
using HBLibrary.Interface.Workspace;
using HBLibrary.Workspace;
using HBLibrary.Interface.IO.Storage.Builder;
using HBLibrary.IO.Storage;
using HBLibrary.IO.Storage.Builder;
using HBLibrary.Interface.Plugins.Builder;
using HBLibrary.Plugins;
using HBLibrary.Core.ChangeTracker;
using FileManager.UI.Models.SettingsModels;

namespace FileManager.UI;
public class UnitySetup : IUnitySetup {
    public void Build(IUnityContainer container) {
        AddToChildContainer(container);
    }

    public static void AddToChildContainer(IUnityContainer container) {
        AddNavigation(container);
        AddLogging(container);

        container.RegisterType<IDialogService, DialogService>();

        AddWorkspace(container);
        
        AddPluginManager(container);
        AddApplicationStorage(container);

        AddSettings(container);
    }

    private static void AddSettings(IUnityContainer container) {
        container.RegisterType<ISettingsService, SettingsService>();

        ISettingsService settingsService = container.Resolve<ISettingsService>();

        // Ensure default settings are applied on first startup
        settingsService.SetIfNullOrNotExists(new SettingsEnvironmentModel());
        settingsService.SetIfNullOrNotExists(new SettingsWinRARModel());
        settingsService.SetIfNullOrNotExists(new SettingsExecutionModel());
    }

    private static void AddNavigation(IUnityContainer container) {
        INavigationStoreBuilder storeBuilder = NavigationStore.CreateBuilder()
            .AddParentTypename(nameof(MainViewModel))
            .AddParentTypename(nameof(ExecutionViewModel))
            .DisposeOnLeave();

        container.RegisterInstance(storeBuilder.Build(), new ContainerControlledLifetimeManager());
        container.RegisterType<INavigationService, NavigationService>();
    }
    private static void AddLogging(IUnityContainer container) {
        ILoggerRegistry registry = LoggerRegistry.FromConfiguration(e => e.Build());
        container.RegisterInstance(registry, new ContainerControlledLifetimeManager());
        container.RegisterType<ILoggerFactory, LoggerFactory>();
    }
    
    private static void AddWorkspace(IUnityContainer container) {
        CommonAppSettings commonAppSettings = container.Resolve<CommonAppSettings>();

        IAccountStorage accountStorage = container.Resolve<IAccountStorage>();

        IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager =
            new ApplicationWorkspaceManager<HBFileManagerWorkspace>(commonAppSettings.ApplicationName!, accountStorage);

        container.RegisterInstance(workspaceManager, new ContainerControlledLifetimeManager());
        container.RegisterSingleton<IWorkspaceLocationManager, WorkspaceLocationManager>();
    }

    private static void AddApplicationStorage(IUnityContainer container) {

        CommonAppSettings commonAppSettings = container.Resolve<CommonAppSettings>();

        string storagePath = Path.Combine(GlobalEnvironment.ApplicationDataBasePath, commonAppSettings.ApplicationName!);

        IApplicationStorageBuilder appStorageBuilder = ApplicationStorage.CreateBuilder(storagePath);

        appStorageBuilder.AddContainer(typeof(SettingsService), b => {
            b.SetContainerPath("settings");

            b.EnableChangeTracker(new ChangeTracker());

            b.ConfigureFileServices(c => {
                c.UseJsonFileService(jfs => {
                    jfs.UseBase64 = true;
                    jfs.SetGlobalOptions(new JsonSerializerOptions {
                        Converters = {
                                new TimeOnlyConverter(),
                            },
                        WriteIndented = true
                    });
                });
            });


            return b.Build();
        });

        container.RegisterInstance(appStorageBuilder.Build(), new ContainerControlledLifetimeManager());
    }

    private static void AddPluginManager(IUnityContainer container) {
        string storagePath = GetPluginStoragePath(container);

        IPluginManagerBuilder builder = PluginManager.CreateBuilder()
            .Configure(e => e.SetPluginsLocation(storagePath))
            .SetDefaultAssemblyLoader()
            .SetDefaultTypeProvider()
            .SetDefaultTypeResolver()
            .SetDefaultTypeRegistry();

        container.RegisterInstance(builder.Build(), new ContainerControlledLifetimeManager());
    }

    public static string GetPluginStoragePath(IUnityContainer container) {
        CommonAppSettings commonAppSettings = container.Resolve<CommonAppSettings>();

        return Path.Combine(GlobalEnvironment.ApplicationDataBasePath,
            commonAppSettings.ApplicationName!,
            "plugins");
    }
}
