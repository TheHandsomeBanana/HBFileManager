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
using HBLibrary.Common;
using HBLibrary.Services.IO.Json;
using HBLibrary.Services.IO;
using HBLibrary.Services.IO.Storage.Builder;
using HBLibrary.Services.IO.Xml;
using HBLibrary.Common.Extensions;
using FileManager.UI.Services.SettingsService;
using FileManager.UI.Services.JobService;
using System.Text.Json;
using HBLibrary.Common.Json;
using FileManager.UI.Converters;
using HBLibrary.Common.Authentication;
using HBLibrary.Common.Account;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using HBLibrary.Common.Authentication.Microsoft;
using HBLibrary.Wpf.Views;
using HBLibrary.Wpf.ViewModels.Login;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Desktop;
using System.Windows.Interop;

namespace FileManager.UI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static bool StateSaved { get; set; } = false;
        public App() {
            this.Exit += (_, _) => SaveApplicationStateOnExit();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => SaveApplicationStateOnExit();
            AppDomain.CurrentDomain.UnhandledException += (_, _) => SaveApplicationStateOnExit();


            IUnityContainer container = UnityBase.CreateChildContainer(nameof(FileManager));
            AddConfiguration(container);
            AddNavigation(container);

            container.RegisterSingleton<IJobService, JobService>();
            container.RegisterType<IDialogService, DialogService>();

            AddAuthentication(container);
            AddApplicationStorage(container);

            container.RegisterType<ISettingsService, SettingsService>(new ContainerControlledLifetimeManager());
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
            NavigationStore navigationStore = new NavigationStore();
            navigationStore.AddDefaultViewModel(nameof(MainViewModel), new ExplorerViewModel());
            navigationStore.AddDefaultViewModel(nameof(SettingsViewModel), new SettingsEnvironmentViewModel());
            container.RegisterInstance<INavigationStore>(navigationStore);
            container.RegisterSingleton<INavigationService, NavigationService>();
        }
        private static void AddAuthentication(IUnityContainer container) {
            AzureAdOptions azureAdOptions = container.Resolve<AzureAdOptions>();
            CommonAppSettings commonAppSettings = container.Resolve<CommonAppSettings>();

            LocalAuthenticationService localAuthenticationService = new LocalAuthenticationService(commonAppSettings.ApplicationName);


            IPublicClientApplication app = PublicClientApplicationBuilder.Create(azureAdOptions.ClientId)
               .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
               .WithRedirectUri(azureAdOptions.RedirectUri)
               .WithWindowsEmbeddedBrowserSupport()
               .WithWindowsDesktopFeatures(new BrokerOptions(BrokerOptions.OperatingSystems.Windows))
               .Build();

            // Token cache for handling accounts across sessions
            MSTokenStorage.Create(commonAppSettings.ApplicationName, app);

            PublicMSAuthenticationService publicMSAuthenticationService
                = new PublicMSAuthenticationService(app, commonAppSettings);

            container.RegisterInstance<ILocalAuthenticationService>(localAuthenticationService, new SingletonLifetimeManager());
            container.RegisterInstance<IPublicMSAuthenticationService>(publicMSAuthenticationService, new SingletonLifetimeManager());


            container.RegisterSingleton<IAccountService, AccountService>();
        }
        private static void AddApplicationStorage(IUnityContainer container) {
            CommonAppSettings commonAppSettings = container.Resolve<CommonAppSettings>();

            string storagePath = Path.Combine(GlobalEnvironment.ApplicationDataBasePath, commonAppSettings.ApplicationName, "data");

            IApplicationStorageBuilder appStorageBuilder = ApplicationStorage.CreateBuilder(storagePath);
            appStorageBuilder.AddContainer(typeof(SettingsService), builder => {
                builder.SetContainerPath("settings");

                builder.ConfigureFileServices(fs => {
                    fs.UseJsonFileService(jfs => {
                        jfs.SetGlobalOptions(new JsonSerializerOptions {
                            WriteIndented = true
                        });

                        jfs.UseBase64 = true;
                    });
                });

                return builder.Build();
            });

            appStorageBuilder.AddContainer(typeof(JobService), builder => {
                builder.SetContainerPath("jobs");

                builder.ConfigureFileServices(fs => {
                    fs.UseJsonFileService(jfs => {
                        jfs.SetGlobalOptions(new JsonSerializerOptions {
                            Converters = {
                                new TimeOnlyConverter(),
                                new JobItemStepConverter()
                            },
                            WriteIndented = true
                        });

                        jfs.UseBase64 = true;
                    });
                });

                return builder.Build();
            });

            container.RegisterInstance(appStorageBuilder.Build(), InstanceLifetime.Singleton);
        }
        #endregion

        #region StateHelper
        public static void SaveApplicationStateOnExit() {
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
        public static void SaveApplicationState() {
            IUnityContainer? container = UnityBase.GetChildContainer(nameof(FileManager));
            if (container is null)
                return;

            IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();
            applicationStorage.SaveAll();
        }

        public bool CanShutdown { get; private set; } = true;
        public void PreventShutdown() {
            CanShutdown = false;
        }

        public void AllowShutdown() {
            CanShutdown = true;
        }
        #endregion

        protected override async void OnStartup(StartupEventArgs e) {
            try {
                IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
                IAccountService accountService = container.Resolve<IAccountService>();
                CommonAppSettings appSettings = container.Resolve<CommonAppSettings>();

                AccountInfo? lastAccount = accountService.GetLastAccount(appSettings.ApplicationName);

                if (lastAccount is not null && lastAccount.AccountType == AccountType.Microsoft) {
                    MainWindow = new MainWindow {
                        DataContext = new MainViewModel()
                    };

                    MainWindow.Show();

                    IntPtr windowHandle = new WindowInteropHelper(MainWindow).Handle;

                    MSAuthCredentials? credentials = MSAuthCredentials
                       .CreateFromParameterStorage(appSettings.ApplicationName, lastAccount.Username, null, b => {
                           b.WithParentActivityOrWindow(windowHandle); // Fallback for interactive login
                       });

                    // Cached credentials have been found, automatically log in using the cached account identifier
                    // -> Do not trigger login UI
                    if (credentials is not null) {
                        await accountService.LoginAsync(credentials, appSettings.ApplicationName);

                        MainWindow.Closed += (sender, _) => {
                            if (CanShutdown) {
                                Shutdown();
                            }
                            else {
                                CanShutdown = true;
                            }
                        };

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
                        MainWindow = new MainWindow {
                            DataContext = new MainViewModel()
                        };

                        MainWindow.Closed += (_, _) => {
                            if (CanShutdown) {
                                Shutdown();
                            }
                            else {
                                CanShutdown = true;
                            }
                        };

                        MainWindow.Show();
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
                SaveApplicationState();
                Shutdown();
            }
        }
    }
}
