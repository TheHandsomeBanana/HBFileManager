using FileManager.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Jobs;
public interface IJobScheduler {
    public event Action<ScheduledJob>? OnJobScheduling;
    public Task Schedule(Job job);
    public Task Shelve(ScheduledJob job);
    public Task<ScheduledJob[]> GetScheduledJobs();
}
