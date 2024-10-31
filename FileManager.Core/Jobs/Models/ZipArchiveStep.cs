using FileManager.Domain.JobSteps;
using HBLibrary.Common;
using HBLibrary.DataStructures;
using HBLibrary.Interface.Plugins.Attributes;
using HBLibrary.Wpf.ViewModels;
using Unity;

namespace FileManager.Core.Jobs.Models;

[Plugin<JobStep>]
[PluginTypeName("ZipArchive")]
[PluginDescription("With this step you can create ZIP archives")]
public class ZipArchiveStep : JobStep {

    public override void Execute(IUnityContainer container) {
        throw new NotImplementedException();
    }

    public override Task ExecuteAsync(IUnityContainer container) {
        throw new NotImplementedException();
    }

    public override System.Windows.Controls.UserControl? GetJobStepView() {
        return null;
    }

    public override IJobStepContext? GetJobStepDataContext() {
        return null;
    }

    public override ImmutableResultCollection Validate(IUnityContainer container) {
        Thread.Sleep(2000);
        return ImmutableResultCollection.Ok();
    }

    public override async Task<ImmutableResultCollection> ValidateAsync(IUnityContainer container) {
        await Task.Delay(2000);
        return ImmutableResultCollection.Ok();
    }
}
