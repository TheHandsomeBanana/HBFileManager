using FileManager.Core.JobSteps.Attributes;
using FileManager.Core.JobSteps.ViewModels;
using HBLibrary.Common;
using HBLibrary.Common.Plugins.Attributes;
using HBLibrary.Wpf.ViewModels;
using Unity;

namespace FileManager.Core.JobSteps.Models;

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
        return ImmutableResultCollection.Ok();
    }

    public override Task<ImmutableResultCollection> ValidateAsync(IUnityContainer container) {
        return Task.FromResult(ImmutableResultCollection.Ok());
    }
}
