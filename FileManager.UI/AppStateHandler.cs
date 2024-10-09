﻿using FileManager.Core.Workspace;
using FileManager.UI.ViewModels;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Workspace;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Services.IO.Storage.Entries;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace FileManager.UI;

public static class AppStateHandler {
    #region State Saving
    public static bool StateSaved { get; set; } = false;
    public static void SaveAppStateOnExit() {
        if (StateSaved) {
            return;
        }

        StateSaved = true;

        IUnityContainer? container = UnityBase.GetChildContainer(nameof(FileManager));
        if (container is null)
            return;

        try {

            IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
            workspaceManager.CurrentWorkspace?.CloseAsync().GetAwaiter().GetResult();

            IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();
            applicationStorage.SaveAll();
        }
        catch {
            // TODO: This only occurs if the login window is closed
        }
    }
    public static void SaveAppState() {
        IUnityContainer? container = UnityBase.GetChildContainer(nameof(FileManager));
        if (container is null)
            return;


        IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        workspaceManager.CurrentWorkspace?.CloseAsync().GetAwaiter().GetResult();

        IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();
        applicationStorage.SaveAll();
    }
    #endregion

    #region User switch
    public static void UserSwitchCallback(bool success) {
        if (success) {
            SaveAppState();

            IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
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

            MainWindow window = new MainWindow(appState) {
                DataContext = new MainViewModel()
            };

            window.Closing += (_, _) => {
                if (AppStateHandler.CanShutdown) {
                    AppStateHandler.SaveAppStateBeforeExit();
                }
            };

            window.Closed += (s, _) => {

                if (AppStateHandler.CanShutdown) {
                    Application.Current.Shutdown();
                }
                else {
                    AppStateHandler.AllowShutdown();
                    if (s is MainWindow { DataContext: IDisposable disposable }) {
                        disposable.Dispose();
                    }

                    INavigationStore store = container.Resolve<INavigationStore>();
                    store.Clear();
                }
            };

            window.Show();
        }
        else {
            Application.Current.Shutdown();
        }
    }
    #endregion


    #region Shutdown Handling
    public static bool CanShutdown { get; private set; } = true;
    public static void PreventShutdown() {
        CanShutdown = false;
    }
    public static void AllowShutdown() {
        CanShutdown = true;
    }

    public static void SaveAppStateBeforeExit() {
        Window mainWindow = Application.Current.MainWindow;

        WindowState windowState = mainWindow.WindowState == WindowState.Minimized
                ? WindowState.Normal
                : mainWindow.WindowState;

        IApplicationStorage applicationStorage = UnityBase.GetChildContainer(nameof(FileManager)).Resolve<IApplicationStorage>();
        applicationStorage.DefaultContainer.AddOrUpdate("appstate", new ApplicationState() {
            WindowState = windowState,
            Top = mainWindow.Top,
            Left = mainWindow.Left
        }, StorageEntryContentType.Json);
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
}
