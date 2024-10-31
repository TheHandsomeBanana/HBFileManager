using FileManager.Domain;
using FileManager.Domain.JobSteps;
using HBLibrary.DataStructures;
using HBLibrary.Interface.IO;
using HBLibrary.Interface.IO.Storage.Container;
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

namespace FileManager.Core.Jobs;
public class JobRunner : IJobRunner {
    private readonly IStorageEntryContainer container;
    private readonly IPluginManager pluginManager;
    private readonly List<JobRun> runningJobs = [];

    public event Action<JobRun>? OnJobStarting;
    public event Action<JobRun>? OnJobFinished;

    public JobRunner(IStorageEntryContainer container, IPluginManager pluginManager) {
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

        OnJobStarting?.Invoke(jobRun);
        jobRun.Start();

        List<Task> asyncJobs = [];
        List<IUnityContainer> asyncJobsContainers = [];
        foreach (StepRun stepRun in jobRun.StepRuns) {
            IUnityContainer tempContainer = CreateTempContainer(stepRun, mainContainer);
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
        OnJobFinished?.Invoke(jobRun);

        foreach (IUnityContainer container in asyncJobsContainers) {
            container.Dispose();
        }
    }

    private static void RunStep(StepRun stepRun, IUnityContainer container) {
        try {
            stepRun.Start(container);
            stepRun.EndSuccess();
        }
        catch (Exception ex) {
            stepRun.EndFailed(ex);
        }
    }

    private static async Task RunStepAsync(StepRun stepRun, IUnityContainer container) {
        try {
            await stepRun.StartAsync(container);
            stepRun.EndSuccess();
        }
        catch (Exception ex) {
            stepRun.EndFailed(ex);
        }
    }

    private IUnityContainer CreateTempContainer(StepRun stepRun, IUnityContainer mainContainer) {
        ILoggerFactory loggerFactory = mainContainer.Resolve<ILoggerFactory>();
        UnityContainer tempContainer = new UnityContainer();

        ILogger tempLogger = loggerFactory.CreateLogger(stepRun.Name, e => e.AddTarget(stepRun.Logs).Build());
        IAsyncLogger tempAsyncLogger = loggerFactory.CreateAsyncLogger(stepRun.Name, e => e.AddTarget(stepRun.Logs).Build());

        tempContainer.RegisterInstance(tempLogger);
        tempContainer.RegisterInstance(tempAsyncLogger);
        tempContainer.RegisterType<IFileEntryService, FileEntryService>();

        return tempContainer;
    }
}
