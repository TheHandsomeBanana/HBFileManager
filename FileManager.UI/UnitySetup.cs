﻿using FileManager.Core.Workspace;
using FileManager.UI.Services.SettingsService;
using FileManager.UI.ViewModels;
using HBLibrary.Common.DI.Unity;
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
using HBLibrary.Common.Plugins.Builder;
using HBLibrary.Common.Plugins;
using HBLibrary.Common;
using HBLibrary.Services.IO.Storage.Builder;
using HBLibrary.Services.IO.Storage;
using System.Text.Json;
using HBLibrary.Common.Json;
using HBLibrary.Common.Authentication.Microsoft;
using HBLibrary.Common.Authentication;
using HBLibrary.Common.Account;
using HBLibrary.Common.Workspace;
using Microsoft.Identity.Client.Desktop;
using HBLibrary.Services.Logging;

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
        container.RegisterType<ISettingsService, SettingsService>();

        AddPluginManager(container);
        AddApplicationStorage(container);
    }

    
    private static void AddNavigation(IUnityContainer container) {
        INavigationStoreBuilder storeBuilder = NavigationStore.CreateBuilder()
            .AddParentTypename(nameof(MainViewModel))
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