namespace FileManager.Core.JobSteps;
public interface IPluginJobStepManager {
    public event Action? PluginJobStepsLoaded;
    public event Action? PluginJobStepLoading;

    public bool JobStepsLoaded { get; }
    public bool JobStepsLoading { get; }
    public void LoadPluginJobSteps();
    public IEnumerable<Type> GetJobStepTypes();
    public Type GetJobStepType(string typeName);
}
