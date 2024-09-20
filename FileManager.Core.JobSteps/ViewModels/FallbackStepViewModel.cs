namespace FileManager.Core.JobSteps.ViewModels;
public class FallbackStepViewModel : JobStepViewModel<IJobStep> {
    public string FallbackText { get; set; }


    public FallbackStepViewModel(IJobStep jobStep) : base(jobStep) {
        JobStepMetadata metadata = Metadata;

        FallbackText = $"No UI available for the step type '{metadata.TypeName}'";
    }
}
