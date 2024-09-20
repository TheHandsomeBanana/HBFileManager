using HBLibrary.Common.Results;

namespace FileManager.Core.JobSteps;

/// <summary>
/// Classes that implement this interface are responsible for executing a job step. They MUST have a parameterless constructor.
/// <br></br>
/// They should also serve as model for the corresponding view if used.
/// </summary>
public interface IJobStep {
    public Guid Id { get; }
    public string Name { get; set; }
    public bool IsAsync { get; set; }
    public void Execute(IServiceProvider serviceProvider);
    public CollectionResult Validate(IServiceProvider serviceProvider);

    public Task ExecuteAsync(IServiceProvider serviceProvider);
    public Task<CollectionResult> ValidateAsync(IServiceProvider serviceProvider);

    public System.Windows.Controls.UserControl? GetJobStepView();
}


