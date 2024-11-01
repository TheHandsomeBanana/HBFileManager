using FileManager.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace FileManager.Core.Jobs;

public interface IJobExecutionManager {
    public event Action<JobRun>? OnJobStarting;
    public event Action<ScheduledJob>? OnJobScheduling;
   
    public JobRun[] GetRunningJobs();
    public Task<JobRun[]> GetCompletedJobsAsync();
    public Task<ScheduledJob[]> GetScheduledJobs();

    public Task RunAsync(Job job, IUnityContainer container);
    public Task Schedule(Job job);
    public Task Shelve(ScheduledJob job);

}
