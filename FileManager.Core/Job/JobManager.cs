using FileManager.Core.JobSteps;
using HBLibrary.Interface.IO.Storage.Container;
using HBLibrary.Interface.IO.Storage.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Job;
public class JobManager : IJobManager {
    private readonly IStorageEntryContainer container;
    public JobManager(IStorageEntryContainer container) {
        this.container = container;
    }

    public void AddOrUpdate(Job job) {
        container.AddOrUpdate(job.Id.ToString(), job, StorageEntryContentType.Json);
    }

    public Job[] GetAll() {
        List<Job> jobs = new List<Job>();
        foreach (IStorageEntry entry in container.GetAll()) {
            if (entry.Get(typeof(Job)) is Job jobItem) {
                jobs.Add(jobItem);
            }
        }
        return [.. jobs];
    }

    public Job? GetById(Guid jobId) {
        if (container.TryGet(jobId.ToString(), out IStorageEntry? entry)) {
            return entry!.Get(typeof(Job)) as Job;
        }

        return null;
    }

    public void Delete(Job jobItem) {
        Delete(jobItem.Id);
    }

    public void Delete(Guid jobId) {
        container.Delete(jobId.ToString());
    }

    public void AddOrUpdateStep(Guid jobId, JobStep step) {
        Job? job = GetById(jobId) ?? throw new InvalidOperationException($"Could not find job with id {jobId}");

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
        Job? job = GetById(jobId)
            ?? throw new InvalidOperationException($"Could not find job with id {jobId}");

        job.Steps.Remove(step);
    }

    public void DeleteStep(Job job, JobStep step) {
        job.Steps.Remove(step);
    }


    public void DeleteStep(JobStep step) {
        Job? foundJob = null;
        JobStep? foundStep = null;

        foreach (Job job in GetAll()) {
            foreach (JobStep jobStep in job.Steps) {
                if (jobStep.Id == step.Id) {
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
        Job? job = GetById(jobId) ?? throw new InvalidOperationException($"Could not find job with id {jobId}");

        return job.Steps.FirstOrDefault(e => e.Id == stepId);
    }

    public JobStep[] GetSteps(Guid jobId) {
        Job? job = GetById(jobId) ?? throw new InvalidOperationException($"Could not find job with id {jobId}");

        return [.. job.Steps];
    }
}
