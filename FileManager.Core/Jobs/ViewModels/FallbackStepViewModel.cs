using FileManager.Core.JobSteps;
using FileManager.Domain.JobSteps;
using HBLibrary.Interface.Plugins;
using HBLibrary.Plugins;

namespace FileManager.Core.Jobs.ViewModels;
public class FallbackStepViewModel : JobStepViewModel<JobStep> {
    public string FallbackText { get; set; }


    public FallbackStepViewModel(JobStep jobStep) : base(jobStep) {
        FallbackText = $"No UI available for the step type '{Metadata.TypeName}'";
    }
}
