using FileManager.Domain.JobSteps;
using HBLibrary.Logging.FlowDocumentTarget;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Unity;

namespace FileManager.Domain;

public class JobRun {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public RunState State { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public TimeSpan? Duration => StartedAt - FinishedAt;
    public StepRun[] StepRuns { get; set; }

    public event Action? OnJobStarting;
    public event Action? OnJobStarted;
    public event Action? OnJobFinished;

    [JsonIgnore]
    public Stopwatch Stopwatch { get; } = new Stopwatch();

    [JsonIgnore]
    public CancellationTokenSource CancellationTokenSource { get; }

    public JobRun(string jobName, StepRun[] stepRuns) {
        State = RunState.Pending;
        Name = jobName;
        StepRuns = stepRuns;
        CancellationTokenSource = new CancellationTokenSource();
    }

    // Json Constructor
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public JobRun() {

    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


    public void Start() {
        OnJobStarting?.Invoke();
        StartedAt = DateTime.UtcNow;
        Stopwatch.Start();
        State = RunState.Running;
        OnJobStarted?.Invoke();
    }

    public void End(RunState? externalState = null) {
        Stopwatch.Stop();
        FinishedAt = DateTime.UtcNow;
        if (externalState.HasValue) {
            State = externalState.Value;
        }
        else {
            foreach (StepRun step in StepRuns) {
                if (State != RunState.CompletedWithWarnings && step.State == RunState.CompletedWithWarnings) {
                    State = RunState.CompletedWithWarnings;
                }

                if (step.State == RunState.Faulted) {
                    State = RunState.Faulted;
                    break;
                }
            }

            if (State == RunState.Running) {
                State = RunState.Success;
            }
        }

        OnJobFinished?.Invoke();
        CancellationTokenSource.Dispose();
    }
}
