﻿using FileManager.Core.JobSteps;
using FileManager.Core.JobSteps.Converters;
using FileManager.Core.Workspace;
using FileManager.UI.Services.SettingsService;
using FileManager.UI.ViewModels;
using FileManager.UI.ViewModels.SettingsViewModels;
using HBLibrary.Common;
using HBLibrary.Common.Account;
using HBLibrary.Common.Authentication;
using HBLibrary.Common.Authentication.Microsoft;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Extensions;
using HBLibrary.Common.Json;
using HBLibrary.Common.Plugins;
using HBLibrary.Common.Plugins.Builder;
using HBLibrary.Common.Workspace;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Services.IO.Storage.Builder;
using HBLibrary.Services.IO.Storage.Entries;
using HBLibrary.Services.IO.Storage.Settings;
using HBLibrary.Services.Logging;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.Services.NavigationService.Builder;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.ViewModels.Login;
using HBLibrary.Wpf.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Desktop;
using Microsoft.IdentityModel.Abstractions;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using Unity;
using Unity.Lifetime;

namespace FileManager.UI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            this.DispatcherUnhandledException += AppOnUnhandledException;
           
            IUnityContainer container = UnityBase.CreateChildContainer(nameof(FileManager));
            AddConfiguration(container);
            AddNavigation(container);
            AddLogging(container);

            container.RegisterType<IDialogService, DialogService>();

            AddAuthentication(container);
            AddWorkspace(container);
            container.RegisterType<ISettingsService, SettingsService>();

            AddPluginManager(container);
            AddApplicationStorage(container);
        }

        private void AppOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            HBDarkMessageBox.Show("Unexpected error", 
                e.Exception.Message,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {            
            AppStateHandler.SaveAppState();
        }

        #region Services
        private static void AddConfiguration(IUnityContainer container) {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                      .SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .Build();

            AzureAdOptions azureAdOptions = new AzureAdOptions();
            configuration.GetSection("AzureAd").Bind(azureAdOptions);

            CommonAppSettings commonAppSettings = new CommonAppSettings();
            configuration.GetSection("Application").Bind(commonAppSettings);

            container.RegisterInstance(azureAdOptions, new SingletonLifetimeManager());
            container.RegisterInstance(commonAppSettings, new SingletonLifetimeManager());
        }
        private static void AddNavigation(IUnityContainer container) {
            INavigationStoreBuilder storeBuilder = NavigationStore.CreateBuilder()
                .DisposeOnLeave();

            container.RegisterInstance(storeBuilder.Build());
            container.RegisterType<INavigationService, NavigationService>();
        }
        private static void AddLogging(IUnityContainer container) {
            ILoggerRegistry registry = LoggerRegistry.FromConfiguration(e => e.Build());
            container.RegisterInstance(registry);
            container.RegisterType<ILoggerFactory, LoggerFactory>();
        }
        private static void AddAuthentication(IUnityContainer container) {
            AzureAdOptions azureAdOptions = container.Resolve<AzureAdOptions>();
            CommonAppSettings commonAppSettings = container.Resolve<CommonAppSettings>();

            LocalAuthenticationService localAuthenticationService = new LocalAuthenticationService();


            IPublicClientApplication app = PublicClientApplicationBuilder.Create(azureAdOptions.ClientId)
               .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
               .WithRedirectUri(azureAdOptions.RedirectUri)
               .WithWindowsEmbeddedBrowserSupport()
               //.WithWindowsDesktopFeatures(new BrokerOptions(BrokerOptions.OperatingSystems.Windows))
               .Build();

            // Token cache for handling accounts across sessions
            MSTokenStorage.Create(commonAppSettings.ApplicationName!, app);

            PublicMSAuthenticationService publicMSAuthenticationService
                = new PublicMSAuthenticationService(app);

            container.RegisterInstance<ILocalAuthenticationService>(localAuthenticationService, new SingletonLifetimeManager());
            container.RegisterInstance<IPublicMSAuthenticationService>(publicMSAuthenticationService, new SingletonLifetimeManager());

            container.RegisterType<IAccountStorage, AccountStorage>();
            container.RegisterSingleton<IAccountService, AccountService>();
        }
        private static void AddWorkspace(IUnityContainer container) {
            CommonAppSettings commonAppSettings = container.Resolve<CommonAppSettings>();

            IAccountStorage accountStorage = container.Resolve<IAccountStorage>();

            IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager =
                new ApplicationWorkspaceManager<HBFileManagerWorkspace>(commonAppSettings.ApplicationName!, accountStorage);

            container.RegisterInstance(workspaceManager, new SingletonLifetimeManager());
            container.RegisterType<IWorkspaceLocationManager, WorkspaceLocationManager>();
        }

        // This is required for user switching
        public static void AddApplicationStorageContainers(IUnityContainer container) {
            IApplicationStorage storage = container.Resolve<IApplicationStorage>();
            IAccountService accountService = container.Resolve<IAccountService>();
            IPluginManager pluginManager = container.Resolve<IPluginManager>();

            string accountId = accountService.Account!.AccountId;

            storage.CreateContainer($"{accountId + nameof(SettingsService)}".ToGuid(), b => {
                b.SetContainerPath($"{Path.Combine(accountId, "settings")}");

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

            appStorageBuilder.AddContainer(typeof(WorkspaceLocationManager), b =>
                b.SetContainerPath("workspacelocations")
                .ConfigureFileServices(fs => {
                    fs.UseJsonFileService();
                })
                .Build()
            );

            container.RegisterInstance(appStorageBuilder.Build(), InstanceLifetime.Singleton);
        }
        
        private static void AddPluginManager(IUnityContainer container) {
            string storagePath = GetPluginStoragePath(container);

            IPluginManagerBuilder builder = PluginManager.CreateBuilder()
                .Configure(e => e.SetPluginsLocation(storagePath))
                .SetDefaultAssemblyLoader()
                .SetDefaultTypeProvider()
                .SetDefaultTypeResolver()
                .SetDefaultTypeRegistry();

            container.RegisterInstance(builder.Build(), InstanceLifetime.Singleton);
        }

        public static string GetPluginStoragePath(IUnityContainer container) {
            CommonAppSettings commonAppSettings = container.Resolve<CommonAppSettings>();

            return Path.Combine(GlobalEnvironment.ApplicationDataBasePath,
                commonAppSettings.ApplicationName!,
                "plugins");
        }
        #endregion


        protected override async void OnStartup(StartupEventArgs e) {
            try {
                IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
                CommonAppSettings appSettings = container.Resolve<CommonAppSettings>();

                // Do not allow multiple main window instances
                if (AppStateHandler.HandleInstanceRunning(this, appSettings.ApplicationName!)) {
                    return;
                }

                IAccountService accountService = container.Resolve<IAccountService>();

                AccountInfo? lastAccount = accountService.AccountStorage.GetLatestAccount(appSettings.ApplicationName!);

                if (lastAccount is not null && lastAccount.AccountType == AccountType.Microsoft) {
                    MSAuthCredentials? credentials = MSAuthCredentials
                       .CreateFromParameterStorage(lastAccount.Username);

                    // Cached credentials have been found, automatically log in using the cached account identifier
                    // -> Do not trigger login UI
                    if (credentials is not null) {
                        await accountService.LoginAsync(credentials, appSettings.ApplicationName!);
                        MainWindowStartup(container);
                        return;
                    }
                }

                StartupLoginViewModel dataContext = new StartupLoginViewModel(accountService, appSettings);

                if (lastAccount is not null
                    && lastAccount.AccountType == AccountType.Local
                    && dataContext.AppLoginContent is LoginViewModel loginViewModel) {

                    loginViewModel.Username = lastAccount.Username;
                }

                StartupLoginWindow loginWindow = new StartupLoginWindow();
                loginWindow.DataContext = dataContext;

                dataContext.StartupCompleted += success => {
                    if (success) {
                        MainWindowStartup(container);
                    }
                    else {
                        loginWindow.Close();
                        Shutdown();
                    }
                };

                loginWindow.ShowDialog();

                base.OnStartup(e);
            }
            catch {
                AppStateHandler.SaveAppStateOnExit();
                Shutdown();
            }
        }

        private void MainWindowStartup(IUnityContainer container) {
            IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();


            ApplicationState appState = new ApplicationState {
                WindowState = WindowState.Normal
            };

            if (applicationStorage.DefaultContainer.TryGet("appstate", out IStorageEntry? entry)) {
                ApplicationState? appStateEntry = entry.Get<ApplicationState>();
                if (appStateEntry != null) {
                    appState = appStateEntry;
                }
            }

            MainWindow = new MainWindow(appState) {
                DataContext = new MainViewModel(),
            };


            MainWindow.Closing += (_, _) => {
                if(AppStateHandler.CanShutdown) {
                    AppStateHandler.SaveAppStateBeforeExit();
                }
            };

            MainWindow.Closed += (_, _) => {
                if (AppStateHandler.CanShutdown) {
                    Shutdown();
                }
                else {
                    // Allow shutdown on next close
                    AppStateHandler.AllowShutdown();
                }
            };

            MainWindow.Show();
        }



        protected override void OnExit(ExitEventArgs e) {
            AppStateHandler.ExitInstance();
            AppStateHandler.SaveAppStateOnExit();

            base.OnExit(e);
        }
    }
}
