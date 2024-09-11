namespace FileManager.Core.JobSteps;
public interface IJobStepManager {
    public void LoadJobSteps();
    public IEnumerable<Type> GetJobStepTypes();
}
