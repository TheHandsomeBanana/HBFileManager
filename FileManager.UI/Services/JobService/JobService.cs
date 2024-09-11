using FileManager.Core.JobSteps;
using FileManager.UI.Models.JobModels;
using HBLibrary.Common.Account;
using HBLibrary.Common.Extensions;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Services.IO.Storage.Container;
using HBLibrary.Services.IO.Storage.Entries;

namespace FileManager.UI.Services.JobService;
public class JobService : IJobService {
    private readonly IStorageEntryContainer container;
    public JobService(IApplicationStorage applicationStorage, IAccountService accountService) {
        string containerString = accountService.Account!.AccountId + nameof(JobService);

        this.container = applicationStorage.GetContainer(containerString.ToGuid());
    }

    public void AddOrUpdate(JobItemModel jobItem) {
        container.AddOrUpdate(jobItem.Id.ToString(), jobItem, StorageEntryContentType.Json);
    }

    public JobItemModel[] GetAll() {
        List<JobItemModel> jobs = new List<JobItemModel>();
        foreach (IStorageEntry entry in container.GetAll()) {
            if (entry.Get(typeof(JobItemModel)) is JobItemModel jobItem) {
                jobs.Add(jobItem);
            }
        }
        return [.. jobs];
    }

    public JobItemModel? GetById(Guid jobId) {
        if (container.TryGet(jobId.ToString(), out IStorageEntry? entry)) {
            return entry!.Get(typeof(JobItemModel)) as JobItemModel;
        }

        return null;
    }

    public void Delete(JobItemModel jobItem) {
        Delete(jobItem.Id);
    }

    public void Delete(Guid jobId) {
        container.Delete(jobId.ToString());
    }

    public void AddOrUpdateStep(Guid jobId, IJobStep step) {
        JobItemModel? job = GetById(jobId) ?? throw new InvalidOperationException($"Could not find job with id {jobId}");
        job.Steps[step.Id] = step;
    }

    public void DeleteStep(Guid jobId, IJobStep step) {
        DeleteStep(jobId, step.Id);
    }

    public void DeleteStep(Guid jobId, Guid stepId) {
        JobItemModel? job = GetById(jobId) ?? throw new InvalidOperationException($"Could not find job with id {jobId}");
        job.Steps.Remove(stepId);
    }

    public IJobStep? GetStepById(Guid jobId, Guid stepId) {
        JobItemModel? job = GetById(jobId) ?? throw new InvalidOperationException($"Could not find job with id {jobId}");

        if (job.Steps.TryGetValue(stepId, out IJobStep? step)) {
            return step;
        }

        return null;
    }

    public IJobStep[] GetSteps(Guid jobId) {
        JobItemModel? job = GetById(jobId) ?? throw new InvalidOperationException($"Could not find job with id {jobId}");

        return [.. job.Steps.Values];
    }
}
