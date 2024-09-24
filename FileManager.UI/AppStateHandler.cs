using HBLibrary.Common.DI.Unity;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Services.IO.Storage.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace FileManager.UI;

public static class AppStateHandler
{
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

        IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();
        applicationStorage.SaveAll();
    }
    public static void SaveAppState() {
        IUnityContainer? container = UnityBase.GetChildContainer(nameof(FileManager));
        if (container is null)
            return;

        IApplicationStorage applicationStorage = container.Resolve<IApplicationStorage>();
        applicationStorage.SaveAll();
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
