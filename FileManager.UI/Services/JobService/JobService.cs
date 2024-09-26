using FileManager.Core.JobSteps;
using FileManager.UI.Models.JobModels;
using HBLibrary.Common.Account;
using HBLibrary.Common.Extensions;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Services.IO.Storage.Container;
using HBLibrary.Services.IO.Storage.Entries;
using System.Diagnostics.Eventing.Reader;
using System.Security.Permissions;

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

    public void AddOrUpdateStep(Guid jobId, JobStep step) {
        JobItemModel? job = GetById(jobId) ?? throw new InvalidOperationException($"Could not find job with id {jobId}");

        if (!job.Steps.Contains(step)) {
            step.ExecutionOrder = job.Steps.LastOrDefault()?.ExecutionOrder ?? 0;
            job.Steps.Add(step);
            return;
        }

        JobStep oldStep = job.Steps.First(e => e.Id == step.Id);

        int index = job.Steps.IndexOf(oldStep);
        job.Steps[index] = step;
    }

    public void DeleteStep(Guid jobId, JobStep step) {
        JobItemModel? job = GetById(jobId) 
            ?? throw new InvalidOperationException($"Could not find job with id {jobId}");

        job.Steps.Remove(step);
    }


    public void DeleteStep(JobStep step) {
        JobItemModel? foundJob = null;
        JobStep? foundStep = null;

        foreach(JobItemModel job in GetAll()) {
            foreach(JobStep jobStep in job.Steps) {
                if(jobStep.Id == step.Id) {
                    foundStep = jobStep;
                    foundJob = job;
                    break;
                }
            }
        }

        if (foundJob is null || foundStep is null) {
            return;
        }

        foundJob?.Steps.Remove(foundStep);
    }

    public JobStep? GetStepById(Guid jobId, Guid stepId) {
        JobItemModel? job = GetById(jobId) ?? throw new InvalidOperationException($"Could not find job with id {jobId}");

        return job.Steps.FirstOrDefault(e => e.Id == stepId);
    }

    public JobStep[] GetSteps(Guid jobId) {
        JobItemModel? job = GetById(jobId) ?? throw new InvalidOperationException($"Could not find job with id {jobId}");

        return [.. job.Steps];
    }

    public void Reorder(JobItemModel[] newValues) {
    }
}
