namespace FileManager.Core.JobSteps;
public interface IJobStepManager {
    public void LoadPluginJobSteps();
    public IEnumerable<Type> GetJobStepTypes();
}
