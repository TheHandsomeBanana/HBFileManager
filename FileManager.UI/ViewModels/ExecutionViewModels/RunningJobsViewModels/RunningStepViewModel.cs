using FileManager.Domain;
using FileManager.Domain.JobSteps;
using HBLibrary.Logging.FlowDocumentTarget;
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

namespace FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
public class RunningStepViewModel : ViewModelBase<StepRun>, IDisposable {

    private readonly DispatcherTimer dispatcherTimer;

    public ListBoxLogTarget LogsTarget => Model.Logs;
    public TimeSpan Elapsed => Model.Stopwatch.Elapsed;
    public string Name => Model.Name;
    public string StepType => Model.StepType;
    public RunState State => Model.State;
    public bool IsRunning => Model.State == RunState.Running;

    public RunningStepViewModel(StepRun model) : base(model) {
        model.OnStepStarting += Model_OnStepStarting;
        model.OnStepFinished += Model_OnStepFinished;

        dispatcherTimer = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        dispatcherTimer.Tick += DispatcherTimer_Tick;
        dispatcherTimer.Start();

    }

    private void Model_OnStepFinished() {
        NotifyPropertyChanged(nameof(State));
        NotifyPropertyChanged(nameof(IsRunning));
    }

    private void Model_OnStepStarting() {
        NotifyPropertyChanged(nameof(State));
        NotifyPropertyChanged(nameof(IsRunning));
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
