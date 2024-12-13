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
using HBLibrary.Logging;
using HBLibrary.Logging.FlowDocumentTarget;
using HBLibrary.Plugins;
using HBLibrary.Wpf.Logging;
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

        JobRun jobRun = new JobRun(job.Name, stepRuns);

        runningJobs.Add(jobRun);

        OnJobStarting?.Invoke(jobRun);
        jobRun.Start();

        List<Task> asyncJobs = [];
        List<IUnityContainer> asyncJobsContainers = [];
        bool canceled = false;
        foreach (StepRun stepRun in jobRun.StepRuns) {
            if(jobRun.CancellationTokenSource.IsCancellationRequested) {
                stepRun.EndCanceled();
                canceled = true;
                continue;
            }

            UnityContainer tempContainer = CreateTempContainer(stepRun, mainContainer);
            if (stepRun.IsAsync) {
                asyncJobs.Add(RunStepAsync(stepRun, tempContainer, jobRun.CancellationTokenSource.Token));
                asyncJobsContainers.Add(tempContainer);
            }
            else {
                RunStep(stepRun, tempContainer, jobRun.CancellationTokenSource.Token);
                tempContainer.Dispose();
            }
        }

        await Task.WhenAll(asyncJobs);
        jobRun.End(canceled || jobRun.CancellationTokenSource.IsCancellationRequested);
        runningJobs.Remove(jobRun);

        container.AddOrUpdate(jobRun.Id.ToString(), jobRun, StorageEntryContentType.Json);
        await container.SaveAsync();

        foreach (IUnityContainer container in asyncJobsContainers) {
            container.Dispose();
        }
    }

    private static void RunStep(StepRun stepRun, UnityContainer container, CancellationToken jobCancellationToken = default) {
        try {
            stepRun.Start(container, jobCancellationToken);

            IExecutionStateHandler stateHandler = container.Resolve<IExecutionStateHandler>();

            switch (stateHandler.State) {
                case RunState.CompletedWithWarnings:
                    stepRun.EndWithWarnings();
                    break;
                case RunState.Canceled:
                    stepRun.EndCanceled();
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

    private static async Task RunStepAsync(StepRun stepRun, UnityContainer container, CancellationToken jobCancellationToken = default) {
        try {
            await stepRun.StartAsync(container, jobCancellationToken);
            IExecutionStateHandler stateHandler = container.Resolve<IExecutionStateHandler>();

            switch (stateHandler.State) {
                case RunState.CompletedWithWarnings:
                    stepRun.EndWithWarnings();
                    break;
                case RunState.Canceled:
                    stepRun.EndCanceled();
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

    private static UnityContainer CreateTempContainer(StepRun stepRun, IUnityContainer mainContainer) {
        UnityContainer tempContainer = new UnityContainer();
        LoggerFactory loggerFactory = new LoggerFactory();

        IExtendedLogger extendedLogger = loggerFactory.CreateExtendedLogger(stepRun.Name, e => e.AddTarget(stepRun.Logs).Build());
        tempContainer.RegisterInstance<ILoggerFactory>(loggerFactory);
        tempContainer.RegisterInstance(extendedLogger);
        
        tempContainer.RegisterType<IFileEntryService, FileEntryService>();
        tempContainer.RegisterType<IExecutionStateHandler, ExecutionStateHandler>(new ContainerControlledLifetimeManager());

        return tempContainer;
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

    public Task<ScheduledJob[]> GetScheduledJobs() {
        throw new NotImplementedException();
    }

    public Task Schedule(Job job) {
        ScheduledJob scheduledJob = new ScheduledJob();
        OnJobScheduling?.Invoke(scheduledJob);

        throw new NotImplementedException();
    }

    public Task Shelve(ScheduledJob job) {
        throw new NotImplementedException();
    }

}
