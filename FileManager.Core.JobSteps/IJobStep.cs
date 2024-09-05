using System.Windows.Controls;
using Unity;

namespace FileManager.Core.JobSteps;
public interface ICommonJobStep {
    /// <summary>
    /// </summary>
    /// <returns>The custom UI provided by the class that implements this interface.</returns>
    public UserControl? GetJobStepView();
}

/// <summary>
/// Classes that implement this interface are responsible for executing a job step.
/// <br></br>
/// They should also serve as model for the corresponding view if used.
/// </summary>
public interface IJobStep : ICommonJobStep {
    public void Execute(IServiceProvider serviceProvider);
    public HBLibrary.Common.Results.ValidationResult Validate(IServiceProvider serviceProvider);
}

/// <summary>
/// Classes that implement this interface are responsible for executing a job step.
/// <br></br>
/// They should also serve as model for the corresponding view if used.
/// </summary>
public interface IAsyncJobStep : ICommonJobStep {
    public Task ExecuteAsync(IServiceProvider serviceProvider);
    public Task<HBLibrary.Common.Results.ValidationResult> ValidateAsync(IServiceProvider serviceProvider);
}
