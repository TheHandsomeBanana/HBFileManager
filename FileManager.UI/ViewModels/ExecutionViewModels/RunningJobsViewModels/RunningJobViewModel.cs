﻿using FileManager.Core.Jobs;
using FileManager.Domain;
using HBLibrary.Logging.FlowDocumentTarget;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
public sealed class RunningJobViewModel : ViewModelBase<JobRun>, IDisposable {
    private readonly DispatcherTimer dispatcherTimer;

    public TimeSpan Elapsed => Model.Stopwatch.Elapsed;

    public string Name => Model.Name;
    public RunState State {
        get => Model.State;
        set {
            Model.State = value;
            NotifyPropertyChanged(nameof(IsRunning));
            NotifyPropertyChanged(nameof(IsSuccess));
            NotifyPropertyChanged(nameof(IsError));
        }
    }

    public ObservableCollection<RunningStepViewModel> RunningSteps { get; set; } = [];

    public bool IsRunning => State == RunState.Running;
    public bool IsSuccess => State == RunState.Success;
    public bool IsError => State == RunState.Faulted;

    public RunningJobViewModel(JobRun model) : base(model) {
        dispatcherTimer = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        dispatcherTimer.Tick += DispatcherTimer_Tick;
        model.OnJobFinished += JobRunner_OnJobFinished;
        dispatcherTimer.Start();
    }

    private void JobRunner_OnJobFinished() {
        dispatcherTimer.Stop();

        Application.Current.Dispatcher.Invoke(() => {
            NotifyPropertyChanged(nameof(IsRunning));
            NotifyPropertyChanged(nameof(IsSuccess));
            NotifyPropertyChanged(nameof(IsError));
        });
    }

    private void DispatcherTimer_Tick(object? sender, EventArgs e) {
        NotifyPropertyChanged(nameof(Elapsed));
    }

    public void Dispose() {
        dispatcherTimer.Stop();
        dispatcherTimer.Tick -= DispatcherTimer_Tick;
        Model.OnJobFinished -= JobRunner_OnJobFinished;
    }
}
