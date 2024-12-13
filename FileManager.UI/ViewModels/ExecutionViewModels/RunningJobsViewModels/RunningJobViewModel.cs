using FileManager.Core.Jobs;
using FileManager.Core.Jobs.Views;
using FileManager.Domain;
using FileManager.Domain.JobSteps;
using FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
using HBLibrary.Interface.Core;
using HBLibrary.Logging.FlowDocumentTarget;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Extensions;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
public sealed class RunningJobViewModel : InitializerViewModelBase<JobRun>, IDisposable {
    private readonly DispatcherTimer dispatcherTimer;

    public TimeSpan Elapsed => Model.Stopwatch.Elapsed;
    public string Name => Model.Name;

    public ObservableCollection<RunningStepViewModel> RunningSteps { get; set; } = [];

    public RunState State => Model.State;

    public bool IsRunning => Model.State == RunState.Running || Model.State == RunState.RunningAsync;
    public bool IsSuccess => Model.State == RunState.Success;
    public bool IsError => Model.State == RunState.Faulted;
    public bool IsWarning => Model.State == RunState.CompletedWithWarnings;
    public bool IsCanceled => Model.State == RunState.Canceled;


    private RunningStepViewModel? selectedStepRun;
    public RunningStepViewModel? SelectedStepRun {
        get { return selectedStepRun; }
        set {
            selectedStepRun = value;
            NotifyPropertyChanged();
        }
    }

    public DateTime? StartedAt {
        get => Model.StartedAt;
        set {
            Model.StartedAt = value;
            NotifyPropertyChanged();
        }
    }

    private bool isCanceling = false;
    public AsyncRelayCommand CancelJobCommand { get; }

    public RunningJobViewModel(JobRun model) : base(model) {
        CancelJobCommand = new AsyncRelayCommand(CancelJob, _ => IsRunning && !isCanceling, OnCancelJobException);

        dispatcherTimer = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        dispatcherTimer.Tick += DispatcherTimer_Tick;
        model.OnJobStarted += JobRun_OnJobStarted;
        model.OnJobFinished += JobRun_OnJobFinished;
        dispatcherTimer.Start();
    }

    private void JobRun_OnJobStarted() {
        Application.Current.Dispatcher.Invoke(() => {
            CancelJobCommand.NotifyCanExecuteChanged();
            NotifyPropertyChanged(nameof(StartedAt));
        });
    }

    private void OnCancelJobException(Exception exception) {
        ApplicationHandler.ShowError("Canceling error", "Could not cancel job - " + exception.Message);
    }

    private async Task CancelJob(object? arg) {
        isCanceling = true;
        Application.Current.Dispatcher.Invoke(CancelJobCommand.NotifyCanExecuteChanged);

        await Model.CancellationTokenSource.CancelAsync();
        isCanceling = false;
    }

    protected override void InitializeViewModel() {
        foreach (StepRun stepRun in Model.StepRuns) {
            RunningStepViewModel stepRunVM = new RunningStepViewModel(stepRun);
            stepRun.OnStepStarting += () => OnStepStarting(stepRunVM);
            stepRun.OnStepFinished += OnStepFinished;
            RunningSteps.Add(stepRunVM);
        }

        SelectedStepRun = RunningSteps.FirstOrDefault();
    }

    private void OnStepStarting(RunningStepViewModel stepRun) {
        if (stepRun.Model.IsAsync) {
            // While there are steps running do not select new async step
            if (RunningSteps.Any(e => e.IsPending || e.IsRunning)) {
                return;
            }
        }

        SelectedStepRun = stepRun;
    }

    private void OnStepFinished() {
        if (RunningSteps.Any(e => !e.Model.IsAsync && (e.IsRunning || e.IsPending))) {
            // If there are sync steps running or pending return;
            return;
        }

        // If there are no more sync steps, select first running async step
        SelectedStepRun = RunningSteps.FirstOrDefault(e => e.Model.IsAsync && e.IsRunning) ?? RunningSteps.Last();
    }

    private void JobRun_OnJobFinished() {
        dispatcherTimer.Stop();

        Application.Current.Dispatcher.Invoke(() => {
            NotifyPropertyChanged(nameof(State));
            NotifyPropertyChanged(nameof(IsRunning));
            NotifyPropertyChanged(nameof(IsSuccess));
            NotifyPropertyChanged(nameof(IsError));
            NotifyPropertyChanged(nameof(IsWarning));
            NotifyPropertyChanged(nameof(IsCanceled));
            CancelJobCommand.NotifyCanExecuteChanged();
        });
    }

    private void DispatcherTimer_Tick(object? sender, EventArgs e) {
        NotifyPropertyChanged(nameof(Elapsed));
    }



    public void Dispose() {
        dispatcherTimer.Stop();

        dispatcherTimer.Tick -= DispatcherTimer_Tick;
        Model.OnJobFinished -= JobRun_OnJobFinished;
        Model.OnJobStarted -= JobRun_OnJobStarted;
        foreach (RunningStepViewModel stepRun in RunningSteps) {
            stepRun.Model.OnStepStarting -= () => OnStepStarting(stepRun);
            stepRun.Model.OnStepFinished -= OnStepFinished;
            stepRun.Dispose();
        }

        this.RunningSteps.Clear();
    }
}
