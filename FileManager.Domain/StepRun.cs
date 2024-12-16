using FileManager.Domain.JobSteps;
using HBLibrary.Interface.Logging;
using HBLibrary.Interface.Logging.Statements;
using HBLibrary.Logging.FlowDocumentTarget;
using HBLibrary.Logging.Statements;
using HBLibrary.Wpf.Logging;
using HBLibrary.Wpf.Logging.Statements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace FileManager.Domain;
public class StepRun {
    private readonly JobStep step;
    public RunState State { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public TimeSpan? Duration => FinishedAt - StartedAt;
    public string StepType { get; set; }
    public bool IsAsync { get; set; }
    public string Name { get; set; }
    public ListBoxLogTarget Logs { get; set; } = new ListBoxLogTarget();

    [JsonIgnore]
    public Stopwatch Stopwatch { get; } = new Stopwatch();

    [JsonIgnore]
    public CancellationTokenSource CancellationTokenSource { get; set; }

    public event Action? OnStepStarting;
    public event Action? OnStepFinished;

    public StepRun(JobStep step, string stepType) {
        CancellationTokenSource = new CancellationTokenSource();
        this.step = step;

        State = RunState.Pending;
        IsAsync = step.IsAsync;
        Name = step.Name;
        StepType = stepType;
    }

    // Json Constructor
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public StepRun()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        
    }

    public bool IsEnabled() {
        return step.IsEnabled;
    }

    public void Start(UnityContainer container, CancellationToken jobCancellationToken = default) {

        StartedAt = DateTime.UtcNow;
        Stopwatch.Start();
        State = RunState.Running;
        OnStepStarting?.Invoke();

        Logs.WriteLogBlock(LogBlockStatement.CreateSeperationBlock());
        Logs.WriteLog(new LogStatement($"{Name} started", Name, LogLevel.Info, DateTime.UtcNow));
        step.Execute(container, CancellationTokenSource.Token, jobCancellationToken);
    }
    
    public Task StartAsync(UnityContainer container, CancellationToken jobCancellationToken = default) {
        StartedAt = DateTime.UtcNow;
        Stopwatch.Start();
        State = RunState.RunningAsync;
        OnStepStarting?.Invoke();

        Logs.WriteLogBlock(LogBlockStatement.CreateSeperationBlock());
        Logs.WriteLog(new LogStatement($"{Name} started asynchronously", Name, LogLevel.Info, DateTime.UtcNow));

        return step.ExecuteAsync(container, CancellationTokenSource.Token, jobCancellationToken);
    }

    public void EndSuccess() {
        Logs.WriteSuccessLog(new LogStatement($"{Name} completed", Name, LogLevel.Info, DateTime.UtcNow));
        FinishedAt = DateTime.UtcNow;
        Stopwatch.Stop();
        State = RunState.Success;
        OnStepFinished?.Invoke();
    }

    public void EndWithWarnings() {
        Logs.WriteSuccessLog(new LogStatement($"{Name} completed with warnings", Name, LogLevel.Warning, DateTime.UtcNow));

        FinishedAt = DateTime.UtcNow;
        Stopwatch.Stop();
        State = RunState.CompletedWithWarnings;
        OnStepFinished?.Invoke();
    }

    public void EndFailed(Exception e) {
        Logs.WriteLog(new LogStatement(e.Message, Name, LogLevel.Fatal, DateTime.UtcNow));

        FinishedAt = DateTime.UtcNow;
        Stopwatch.Stop();
        State = RunState.Faulted;
        OnStepFinished?.Invoke();
    }
    
    public void EndCanceled() {
        Logs.WriteLog(new LogStatement($"Canceled executing {Name}", Name, LogLevel.Warning, DateTime.UtcNow));

        FinishedAt = DateTime.UtcNow;
        Stopwatch.Stop();
        State = RunState.Canceled;
        OnStepFinished?.Invoke();
    }
    
    public void Skip() {
        Logs.WriteLog(new LogStatement($"This step is not enabled, execution skipped.", Name, LogLevel.Info, DateTime.UtcNow));
        State = RunState.Skipped;
        OnStepFinished?.Invoke();
    }
}
