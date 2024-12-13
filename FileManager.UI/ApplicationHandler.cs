using FileManager.Core;
using FileManager.Core.Workspace;
using FileManager.UI.ViewModels;
using HBLibrary.DI;
using HBLibrary.Interface.IO.Storage;
using HBLibrary.Interface.IO.Storage.Entries;
using HBLibrary.Interface.Workspace;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using Microsoft.Graph.Models.TermStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace FileManager.UI;

public static class ApplicationHandler {
    public static readonly Guid FileManagerContainerGuid = Guid.NewGuid();

    static ApplicationHandler() {
        DIContainerGuids.FileManagerContainerGuid = FileManagerContainerGuid;
    }

    #region State Saving
    public static bool StateSaved { get; set; } = false;
    public static void SaveAppStateOnExit() {
        if (StateSaved) {
            return;
        }

        StateSaved = true;

        if (UnityBase.Registry.TryGet(FileManagerContainerGuid, out IUnityContainer? container)) {
            IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
            workspaceManager.CurrentWorkspace?.Close();

            IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();
            applicationStorage.SaveAll();
        }
    }

    public static void SaveAppState() {
        IUnityContainer container = UnityBase.Registry.Get(FileManagerContainerGuid);

        IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        workspaceManager.CurrentWorkspace?.Save();

        IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();
        applicationStorage.SaveAll();
    }

    public static void SaveAppStateBeforeExit() {
        Window mainWindow = Application.Current.MainWindow;

        WindowState windowState = mainWindow.WindowState == WindowState.Minimized
                ? WindowState.Normal
                : mainWindow.WindowState;

        IApplicationStorage applicationStorage = UnityBase.Registry.Get(FileManagerContainerGuid).Resolve<IApplicationStorage>();
        applicationStorage.DefaultContainer.AddOrUpdate("appstate", new ApplicationState() {
            WindowState = windowState,
            Top = mainWindow.Top,
            Left = mainWindow.Left
        }, StorageEntryContentType.Json);
    }
    #endregion

    #region DI

    public static void AddBaseDIContainer() {
        UnityBase.Boot(new UnityBaseSetup());
    }

    public static void AddChildContainer() {
        IUnityContainer container = UnityBase.Registry.RegisterChildContainer(FileManagerContainerGuid);
        UnityBase.Boot(container, new UnitySetup());
    }

    public static void DisposeChildContainer() {
        UnityBase.Registry.DisposeChildContainer(FileManagerContainerGuid);
    }
    #endregion

    #region Account switch
    public static bool CanShutdown { get; private set; } = true;

    public static void OnAccountSwitching() {
        CanShutdown = false;
        SaveAppState();
        DisposeChildContainer();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
    public static void AllowShutdown() {
        CanShutdown = true;
    }

    public static void OnAccountSwitched(bool success) {
        if (success) {
            AddChildContainer();

            IUnityContainer container = UnityBase.Registry.Get(FileManagerContainerGuid);
            IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();

            ApplicationState appState = new ApplicationState {
                WindowState = WindowState.Normal,
            };

            if (applicationStorage.DefaultContainer.TryGet("appstate", out IStorageEntry? entry)) {
                ApplicationState? appStateEntry = entry.Get<ApplicationState>();
                if (appStateEntry != null) {
                    appState = appStateEntry;
                }
            }

            Application.Current.MainWindow = CreateNewMainWindow(container, appState);
            Application.Current.MainWindow.Show();
        }
        else {
            Application.Current.Shutdown();
        }
    }

    public static MainWindow CreateNewMainWindow(IUnityContainer container, ApplicationState appState) {
        MainWindow mainWindow = new MainWindow(appState) {
            DataContext = new MainViewModel(),
        };


        mainWindow.Closing += (_, _) => {
            if (ApplicationHandler.CanShutdown) {
                ApplicationHandler.SaveAppStateBeforeExit();
            }
        };

        mainWindow.Closed += (s, e) => {
            if (ApplicationHandler.CanShutdown) {
                Application.Current.Shutdown();
            }
            else {
                // Allow shutdown on next close
                ApplicationHandler.AllowShutdown();
                if (s is MainWindow mainWindow) {
                    ((MainViewModel)mainWindow.DataContext).Dispose();
                    mainWindow.DataContext = null;
                }
            }
        };

        return mainWindow;
    }
    #endregion

    #region Single Instance Handling
    private static Mutex? mutex;

    // Logic to bring existing window to front
    // Windows API for sending messages
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;
    private static void Focus() {
        IntPtr hWnd = FindWindow("", "HB File Manager");

        if (hWnd != IntPtr.Zero) {
            // If the window is minimized, restore it
            if (IsIconic(hWnd)) {
                ShowWindow(hWnd, SW_RESTORE);
            }

            // Bring the window to the foreground
            SetForegroundWindow(hWnd);
        }
    }


    public static bool HandleInstanceRunning(Application application, string appName) {
        mutex = new Mutex(true, appName, out bool createdNew);
        if (!createdNew) {
            Focus();
            application.Shutdown();
        }

        return !createdNew;
    }
    #endregion

    public static void ExitInstance() {
        mutex?.Dispose();
        mutex = null;
    }

    public static void ShowError(string title, string message) {
        HBDarkMessageBox.Show(title, message, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
