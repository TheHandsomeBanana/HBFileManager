using HBLibrary.Common;
using HBLibrary.Wpf.ViewModels;
using System.Windows.Controls;

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
    public bool IsEnabled { get; set; }
    public int ExecutionOrder { get; set; }
    public void Execute(IServiceProvider serviceProvider);
    public ImmutableResultCollection Validate(IServiceProvider serviceProvider);

    public Task ExecuteAsync(IServiceProvider serviceProvider);
    public Task<ImmutableResultCollection> ValidateAsync(IServiceProvider serviceProvider);

    public UserControl? GetJobStepView();
    public ViewModelBase? GetJobStepDataContext();
}


