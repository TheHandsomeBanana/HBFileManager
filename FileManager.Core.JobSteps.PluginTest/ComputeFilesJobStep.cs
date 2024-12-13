using FileManager.Domain.JobSteps;
using HBLibrary.DataStructures;
using HBLibrary.Interface.Plugins.Attributes;
using HBLibrary.Wpf.ViewModels;
using System.Windows.Controls;
using Unity;

namespace FileManager.Core.JobSteps.PluginTest;

[Plugin<JobStep>]
[PluginTypeName("ComputeFiles")]
[PluginDescription("Used to compute numerous files.")]
public class ComputeFilesJobStep : JobStep {
    public override void Execute(IUnityContainer container, CancellationToken stepCancellationToken = default, CancellationToken jobCancellationToken = default) {
        Thread.Sleep(2000);
        throw new StepExecutionException("Step execution failed", false);
    }

    public override async Task ExecuteAsync(IUnityContainer container, CancellationToken stepCancellationToken = default, CancellationToken jobCancellationToken = default) {
        await Task.Delay(2000);
        throw new StepExecutionException("Step execution failed", false);
    }

    public override UserControl? GetJobStepView() {
        return null;
    }

    public override IJobStepContext? GetJobStepDataContext() {
        return null;
    }

    public override ImmutableResultCollection Validate(IUnityContainer container) {
        return ImmutableResultCollection.Ok();
    }

    public override Task<ImmutableResultCollection> ValidateAsync(IUnityContainer container) {
        return Task.FromResult(ImmutableResultCollection.Ok());
    }
}
