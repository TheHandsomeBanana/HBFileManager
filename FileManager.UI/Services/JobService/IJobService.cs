using FileManager.Core.JobSteps;
using FileManager.UI.Models.JobModels;

namespace FileManager.UI.Services.JobService;
public interface IJobService {
    public void AddOrUpdate(JobItemModel jobItem);
    public void Delete(JobItemModel jobItem);
    public void Delete(Guid jobId);
    public JobItemModel[] GetAll();
    public JobItemModel? GetById(Guid jobId);

    public void AddOrUpdateStep(Guid jobId, JobStep step);
    public void DeleteStep(Guid jobId, JobStep step);
    public void DeleteStep(JobItemModel job, JobStep step);
    public void DeleteStep(JobStep step);
    public JobStep? GetStepById(Guid jobId, Guid stepId);
    public JobStep[] GetSteps(Guid jobId);
}
