using FileManager.Domain.JobSteps;
using HBLibrary.Interface.Logging;
using HBLibrary.Interface.Logging.Statements;
using HBLibrary.Logging.FlowDocumentTarget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace FileManager.Domain;
public class StepRun {
    private readonly JobStep step;
    public RunState State { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public TimeSpan? Duration => StartedAt - FinishedAt;
    public string StepType { get; set; }
    public bool IsAsync { get; set; }
    public string Name { get; set; }
    public FlowDocumentTarget Logs { get; set; } = new FlowDocumentTarget();
    

    public StepRun(JobStep step, string stepType) {
        this.step = step;

        State = RunState.Pending;
        IsAsync = step.IsAsync;
        Name = step.Name;
        StepType = stepType;
    }

    public void Start(IUnityContainer container) {
        StartedAt = DateTime.UtcNow;
        State = RunState.Running;

        step.Execute(container);
    }
    
    public Task StartAsync(IUnityContainer container) {
        StartedAt = DateTime.UtcNow;
        State = RunState.Running;

        return step.ExecuteAsync(container);
    }

    public void EndSuccess() {
        Logs.WriteSuccessLog(new LogStatement($"Step {Name} executed successfully", Name, LogLevel.Fatal, DateTime.UtcNow));
        FinishedAt = DateTime.UtcNow;
        State = RunState.Success;
    }

    public void EndFailed(Exception e) {
        Logs.WriteLog(new LogStatement(e.Message, Name, LogLevel.Fatal, DateTime.UtcNow));
        FinishedAt = DateTime.UtcNow;
        State = RunState.Faulted;
    }
}
