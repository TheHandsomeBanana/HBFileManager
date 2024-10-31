using FileManager.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace FileManager.Core.Jobs;

public interface IJobRunner {
    public event Action<JobRun>? OnJobStarting;
    public event Action<JobRun>? OnJobFinished;
    public JobRun[] GetRunningJobs();
    public Task RunAsync(Job job, IUnityContainer container);
}
