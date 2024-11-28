using FileManager.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Jobs;
public interface IJobHistoryManager {
    public Task<JobRun[]> GetCompletedJobsAsync();
    public void DeleteJob(Guid id);
    public Task DeleteJobAsync(Guid id);
    public Task ClearJobsAsync();
}
