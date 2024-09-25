using HBLibrary.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileManager.Core.JobSteps;

/// <summary>
/// Classes that implement this abstract class are responsible for executing a job step. They MUST have a parameterless constructor.
/// <br></br>
/// They should also serve as model for the corresponding view if used.
/// </summary>
public abstract class JobStep : IJobStep {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public bool IsAsync { get; set; }
    public bool IsEnabled { get; set; }
    public int ExecutionOrder { get; set; }

    public abstract void Execute(IServiceProvider serviceProvider);
    public abstract Task ExecuteAsync(IServiceProvider serviceProvider);
    public abstract UserControl? GetJobStepView(bool createDataContext);
    public abstract ImmutableResultCollection Validate(IServiceProvider serviceProvider);
    public abstract Task<ImmutableResultCollection> ValidateAsync(IServiceProvider serviceProvider);
}
