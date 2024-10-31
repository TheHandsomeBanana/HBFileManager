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

    [JsonIgnore]
    public Stopwatch Stopwatch { get; } = new Stopwatch();

    public JobRun(Job job, StepRun[] stepRuns) {
        State = RunState.Pending;
        Name = job.Name;
        StepRuns = stepRuns;
    }


    public void Start() {
        StartedAt = DateTime.UtcNow;
        Stopwatch.Start();
        State = RunState.Running;
    }

    public void End() {
        Stopwatch.Stop();
        FinishedAt = DateTime.UtcNow;

        if (StepRuns.Any(e => e.State == RunState.Faulted)) {
            State = RunState.Faulted;
        }
        else {
            State = RunState.Success;
        }
    }
}
