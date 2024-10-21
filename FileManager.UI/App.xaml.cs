using HBLibrary.Common;
using HBLibrary.Common.Json;
using HBLibrary.Common.Plugins;
using HBLibrary.Core;
using HBLibrary.DI;
using HBLibrary.Interface.IO.Storage;
using HBLibrary.Interface.IO.Storage.Entries;
using HBLibrary.Interface.Security.Account;
using HBLibrary.Security.Authentication;
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
        public const string ViewModelContainer = "ViewModelContainer";

        static App() {
            ApplicationHandler.AddBaseDIContainer();
            ApplicationHandler.AddChildContainer();
        }

        public App() {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            this.DispatcherUnhandledException += AppOnUnhandledException;
        }

        private void AppOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            HBDarkMessageBox.Show("Unexpected error", 
                e.Exception.Message,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {            
            ApplicationHandler.SaveAppState();
        }


        protected override async void OnStartup(StartupEventArgs e) {
            try {
                IUnityContainer container = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);
                CommonAppSettings appSettings = container.Resolve<CommonAppSettings>();

                // Do not allow multiple main window instances
                if (ApplicationHandler.HandleInstanceRunning(this, appSettings.ApplicationName!)) {
                    return;
                }

                IAccountService accountService = container.Resolve<IAccountService>();

                IAccountInfo? lastAccount = accountService.AccountStorage.GetLatestAccount(appSettings.ApplicationName!);

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
                ApplicationHandler.SaveAppStateOnExit();
                Shutdown();
            }
        }

        private void MainWindowStartup(IUnityContainer container) {
            IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();
            INavigationStore store = container.Resolve<INavigationStore>();

            ApplicationState appState = new ApplicationState {
                WindowState = WindowState.Normal
            };

            if (applicationStorage.DefaultContainer.TryGet("appstate", out IStorageEntry? entry)) {
                ApplicationState? appStateEntry = entry.Get<ApplicationState>();
                if (appStateEntry != null) {
                    appState = appStateEntry;
                }
            }

            MainWindow = ApplicationHandler.CreateNewMainWindow(container, appState);

            MainWindow.Show();
        }



        protected override void OnExit(ExitEventArgs e) {
            ApplicationHandler.ExitInstance();
            ApplicationHandler.SaveAppStateOnExit();

            base.OnExit(e);
        }
    }
}
