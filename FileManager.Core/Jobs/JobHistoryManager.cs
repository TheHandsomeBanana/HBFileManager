using FileManager.Domain;
using HBLibrary.Interface.IO.Storage.Container;
using HBLibrary.Interface.IO.Storage.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Jobs;
public class JobHistoryManager : IJobHistoryManager {
    private IStorageEntryContainer container;
    public JobHistoryManager(IStorageEntryContainer container) {
        this.container = container;
    }


    public Task ClearJobsAsync() {
        return container.DeleteAllAsync();
    }

    public Task DeleteJobAsync(Guid id) {
        return container.DeleteAsync(id.ToString());
    }
    
    public void DeleteJob(Guid id) {
        container.Delete(id.ToString());
    }

    public async Task<JobRun[]> GetCompletedJobsAsync() {
        List<Task<JobRun?>> entryGetTasks = [];
        foreach (IStorageEntry entry in container.GetAll()) {
            entryGetTasks.Add(entry.GetAsync<JobRun>());
        }


        JobRun?[] jobRuns = await Task.WhenAll(entryGetTasks);
        return jobRuns.Where(e => e is not null)
            .ToArray()!;
    }
}
