using FileManager.Core.Jobs;
using FileManager.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core;
public class JobScheduler : IJobScheduler {
    public event Action<ScheduledJob>? OnJobScheduling;

    public Task<ScheduledJob[]> GetScheduledJobs() {
        throw new NotImplementedException();
    }

    public Task Schedule(Job job) {
        throw new NotImplementedException();
    }

    public Task Shelve(ScheduledJob job) {
        throw new NotImplementedException();
    }
}
