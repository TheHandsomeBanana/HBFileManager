using FileManager.Domain;
using FileManager.Domain.JobSteps;
using FileManager.UI.Models.SettingsModels;
using FileManager.UI.Services.SettingsService;
using HBLibrary.DI;
using HBLibrary.Logging.FlowDocumentTarget;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Extensions;
using HBLibrary.Wpf.Logging;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Unity;

namespace FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
public class RunningStepViewModel : ViewModelBase<StepRun>, IDisposable {

    private readonly DispatcherTimer dispatcherTimer;

    public ListBoxLogTarget LogsTarget => Model.Logs;
    public TimeSpan Elapsed => Model.Stopwatch.Elapsed;
    public string Name => Model.Name;
    public string StepType => Model.StepType;
    public RunState State => Model.State;
    public bool IsPending => Model.State == RunState.Pending;
    public bool IsRunning => Model.State == RunState.Running || Model.State == RunState.RunningAsync;
    public bool IsSuccess => Model.State == RunState.Success;
    public bool IsError => Model.State == RunState.Faulted;
    public bool IsWarning => Model.State == RunState.CompletedWithWarnings;
    public bool IsCanceled => Model.State == RunState.Canceled;


    public bool ShowTimestamp { get; }
    public bool ShowExplicitLevel { get; }
    public bool ShowCategory { get; }


    private bool isCanceling = false;
    public AsyncRelayCommand CancelStepCommand { get; }

    public RunningStepViewModel(StepRun model) : base(model) {
        CancelStepCommand = new AsyncRelayCommand(CancelStep, _ => IsRunning && !isCanceling, OnCancelException);

        model.OnStepStarting += Model_OnStepStarting;
        model.OnStepFinished += Model_OnStepFinished;

        dispatcherTimer = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        dispatcherTimer.Tick += DispatcherTimer_Tick;
        dispatcherTimer.Start();

        IUnityContainer container = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);

        ISettingsService settingsService = container.Resolve<ISettingsService>();
        SettingsEnvironmentModel? environmentSettings = settingsService.GetSetting<SettingsEnvironmentModel>();
        if (environmentSettings is not null) {
            ShowExplicitLevel = environmentSettings.ShowLogLevelInRunLogs;
            ShowTimestamp = environmentSettings.ShowTimestampInRunLogs;
            ShowCategory = environmentSettings.ShowCategoryInRunLogs;
        }
    }

    private void OnCancelException(Exception exception) {
        ApplicationHandler.ShowError("Canceling error", "Could not cancel step - " + exception.Message);
    }

    private async Task CancelStep(object? arg) {
        isCanceling = true;
        Application.Current.Dispatcher.Invoke(CancelStepCommand.NotifyCanExecuteChanged);
        await Model.CancellationTokenSource.CancelAsync();
        isCanceling = false;
    }

    private void Model_OnStepFinished() {
        NotifyPropertyChanged(nameof(State));
        NotifyPropertyChanged(nameof(IsRunning));
        NotifyPropertyChanged(nameof(IsError));
        NotifyPropertyChanged(nameof(IsSuccess));
        NotifyPropertyChanged(nameof(IsWarning));
        NotifyPropertyChanged(nameof(IsCanceled));

        Application.Current.Dispatcher.Invoke(CancelStepCommand.NotifyCanExecuteChanged);
    }

    private void Model_OnStepStarting() {
        NotifyPropertyChanged(nameof(State));
        NotifyPropertyChanged(nameof(IsRunning));
        NotifyPropertyChanged(nameof(IsError));
        NotifyPropertyChanged(nameof(IsSuccess));
        NotifyPropertyChanged(nameof(IsWarning));
        NotifyPropertyChanged(nameof(IsCanceled));

        Application.Current.Dispatcher.Invoke(CancelStepCommand.NotifyCanExecuteChanged);
    }

    private void DispatcherTimer_Tick(object? sender, EventArgs e) {
        NotifyPropertyChanged(nameof(Elapsed));
    }

    public void Dispose() {
        dispatcherTimer.Tick -= DispatcherTimer_Tick;
        Model.OnStepStarting -= Model_OnStepStarting;
        Model.OnStepFinished -= Model_OnStepFinished;
    }
}
