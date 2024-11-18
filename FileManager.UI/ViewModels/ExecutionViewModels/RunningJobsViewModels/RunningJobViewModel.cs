using FileManager.Core.Jobs;
using FileManager.Core.Jobs.Views;
using FileManager.Domain;
using FileManager.Domain.JobSteps;
using FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
using HBLibrary.Interface.Core;
using HBLibrary.Logging.FlowDocumentTarget;
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

    public bool IsRunning => Model.State == RunState.Running;
    public bool IsSuccess => Model.State == RunState.Success;
    public bool IsError => Model.State == RunState.Faulted;
    public bool IsWarning => Model.State == RunState.CompletedWithWarnings;


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

    public RunningJobViewModel(JobRun model) : base(model) {
        dispatcherTimer = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        dispatcherTimer.Tick += DispatcherTimer_Tick;
        model.OnJobStarting += Model_OnJobStarting;
        model.OnJobFinished += JobRun_OnJobFinished;
        dispatcherTimer.Start();
    }

    private void Model_OnJobStarting() {
        NotifyPropertyChanged(nameof(StartedAt));
    }

    protected override void InitializeViewModel() {
        foreach(StepRun stepRun in Model.StepRuns) {
            RunningStepViewModel stepRunVM = new RunningStepViewModel(stepRun);
            stepRun.OnStepStarting += () => OnStepStarting(stepRunVM);
            RunningSteps.Add(stepRunVM);
        }
    }

    private void OnStepStarting(RunningStepViewModel stepRun) {
        SelectedStepRun = stepRun;
    }

    private void JobRun_OnJobFinished() {
        dispatcherTimer.Stop();

        Application.Current.Dispatcher.Invoke(() => {
            NotifyPropertyChanged(nameof(State));
            NotifyPropertyChanged(nameof(IsRunning));
            NotifyPropertyChanged(nameof(IsSuccess));
            NotifyPropertyChanged(nameof(IsError));
            NotifyPropertyChanged(nameof(IsWarning));
        });
    }

    private void DispatcherTimer_Tick(object? sender, EventArgs e) {
        NotifyPropertyChanged(nameof(Elapsed));
    }

   

    public void Dispose() {
        dispatcherTimer.Stop();

        dispatcherTimer.Tick -= DispatcherTimer_Tick;
        Model.OnJobFinished -= JobRun_OnJobFinished;
        foreach (RunningStepViewModel stepRun in RunningSteps) {
            stepRun.Model.OnStepStarting -= () => OnStepStarting(stepRun);
            stepRun.Dispose();
        }

        this.RunningSteps.Clear();
    }
}
