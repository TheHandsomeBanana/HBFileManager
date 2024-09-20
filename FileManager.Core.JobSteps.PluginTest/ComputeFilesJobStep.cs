using FileManager.Core.JobSteps.Attributes;
using System.Windows.Controls;

namespace FileManager.Core.JobSteps.PluginTest;

[JobStepType("ComputeFiles")]
[JobStepDescription("Used to compute numerous files.")]
public class ComputeFilesJobStep : IJobStep {
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";
    public bool IsAsync { get; set; }

    public void Execute(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public Task ExecuteAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public UserControl? GetJobStepView() {
        return null;
    }

    public HBLibrary.Common.Results.Result Validate(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public Task<HBLibrary.Common.Results.Result> ValidateAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }
}
