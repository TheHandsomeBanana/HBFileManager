using FileManager.Core.JobSteps;
using FileManager.UI.Models.JobModels;

namespace FileManager.UI.Services.JobService;
public interface IJobService {
    public void AddOrUpdate(JobItemModel jobItem);
    public void Delete(JobItemModel jobItem);
    public void Delete(Guid jobId);
    public JobItemModel[] GetAll();
    public JobItemModel? GetById(Guid jobId);

    public void AddOrUpdateStep(Guid jobId, IJobStep step);
    public void DeleteStep(Guid jobId, IJobStep step);
    public void DeleteStep(Guid jobId, Guid stepId);
    public IJobStep? GetStepById(Guid jobId, Guid stepId);
    public IJobStep[] GetSteps(Guid jobId);
}
