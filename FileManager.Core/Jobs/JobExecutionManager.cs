using FileManager.Core.JobSteps;
using FileManager.Domain;
using FileManager.Domain.JobSteps;
using HBLibrary.DataStructures;
using HBLibrary.Interface.IO;
using HBLibrary.Interface.IO.Storage.Container;
using HBLibrary.Interface.IO.Storage.Entries;
using HBLibrary.Interface.Logging;
using HBLibrary.Interface.Plugins;
using HBLibrary.IO;
using HBLibrary.Logging.FlowDocumentTarget;
using HBLibrary.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unity;
using Unity.Lifetime;

namespace FileManager.Core.Jobs;
public class JobExecutionManager : IJobExecutionManager {
    private readonly IStorageEntryContainer container;
    private readonly IPluginManager pluginManager;
    private readonly List<JobRun> runningJobs = [];

    public event Action<JobRun>? OnJobStarting;
    public event Action<ScheduledJob>? OnJobScheduling;

   
    public JobExecutionManager(IStorageEntryContainer container, IPluginManager pluginManager) {
        this.container = container;
        this.pluginManager = pluginManager;
    }

    public JobRun[] GetRunningJobs() {
        return [.. runningJobs];
    }

    public async Task RunAsync(Job job, IUnityContainer mainContainer) {
        StepRun[] stepRuns = job.Steps.Select(e => new StepRun(e, pluginManager.GetStaticPluginMetadata(e.GetType()).TypeName))
                .ToArray();

        JobRun jobRun = new JobRun(job, stepRuns);
        runningJobs.Add(jobRun);

        OnJobStarting?.Invoke(jobRun);
        jobRun.Start();

        List<Task> asyncJobs = [];
        List<IUnityContainer> asyncJobsContainers = [];
        foreach (StepRun stepRun in jobRun.StepRuns) {
            UnityContainer tempContainer = CreateTempContainer(stepRun, mainContainer);
            if (stepRun.IsAsync) {
                asyncJobs.Add(RunStepAsync(stepRun, tempContainer));
                asyncJobsContainers.Add(tempContainer);
            }
            else {
                RunStep(stepRun, tempContainer);
                tempContainer.Dispose();
            }
        }

        await Task.WhenAll(asyncJobs);
        jobRun.End();
        runningJobs.Remove(jobRun);

        container.AddOrUpdate(jobRun.Id.ToString(), jobRun, StorageEntryContentType.Json);


        foreach (IUnityContainer container in asyncJobsContainers) {
            container.Dispose();
        }
    }

    private static void RunStep(StepRun stepRun, UnityContainer container) {
        try {
            stepRun.Start(container);

            IExecutionStateHandler stateHandler = container.Resolve<IExecutionStateHandler>();

            switch (stateHandler.State) {
                case RunState.CompletedWithWarnings:
                    stepRun.EndWithWarnings();
                    break;
                default:
                    stepRun.EndSuccess();
                    break;
            }

        }
        catch (Exception ex) {
            stepRun.EndFailed(ex);
        }
    }

    private static async Task RunStepAsync(StepRun stepRun, UnityContainer container) {
        try {
            await stepRun.StartAsync(container);
            stepRun.EndSuccess();
        }
        catch (Exception ex) {
            stepRun.EndFailed(ex);
        }
    }

    private static UnityContainer CreateTempContainer(StepRun stepRun, IUnityContainer mainContainer) {
        ILoggerFactory loggerFactory = mainContainer.Resolve<ILoggerFactory>();
        UnityContainer tempContainer = new UnityContainer();

        ILogger tempLogger = loggerFactory.CreateLogger(stepRun.Name, e => e.AddTarget(stepRun.Logs).Build());
        IAsyncLogger tempAsyncLogger = loggerFactory.CreateAsyncLogger(stepRun.Name, e => e.AddTarget(stepRun.Logs).Build());

        tempContainer.RegisterInstance(tempLogger);
        tempContainer.RegisterInstance(tempAsyncLogger);
        tempContainer.RegisterType<IFileEntryService, FileEntryService>();

        tempContainer.RegisterType<IExecutionStateHandler, ExecutionStateHandler>(new ContainerControlledLifetimeManager());

        return tempContainer;
    }

    public async Task<JobRun[]> GetCompletedJobsAsync() {
        List<Task<JobRun?>> entryGetTasks = [];
        foreach(IStorageEntry entry in container.GetAll()) {
            entryGetTasks.Add(entry.GetAsync<JobRun>());    
        }

        JobRun?[] jobRuns = await Task.WhenAll(entryGetTasks);
        return jobRuns.Where(e => e is not null)
            .ToArray()!;
    }

    public Task<ScheduledJob[]> GetScheduledJobs() {
        throw new NotImplementedException();
    }

    public async Task Schedule(Job job) {
        ScheduledJob scheduledJob = new ScheduledJob();
        OnJobScheduling?.Invoke(scheduledJob);
    }

    public Task Shelve(ScheduledJob job) {
        throw new NotImplementedException();
    }

}
