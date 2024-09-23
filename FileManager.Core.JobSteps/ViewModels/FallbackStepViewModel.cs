using HBLibrary.Common.Plugins;

namespace FileManager.Core.JobSteps.ViewModels;
public class FallbackStepViewModel : JobStepViewModel<JobStep> {
    public string FallbackText { get; set; }


    public FallbackStepViewModel(JobStep jobStep) : base(jobStep) {
        PluginMetadata metadata = Metadata;

        FallbackText = $"No UI available for the step type '{metadata.TypeName}'";
    }
}
