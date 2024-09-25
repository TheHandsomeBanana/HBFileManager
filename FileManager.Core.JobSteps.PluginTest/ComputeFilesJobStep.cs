using FileManager.Core.JobSteps.Attributes;
using HBLibrary.Common;
using HBLibrary.Common.Plugins.Attributes;
using System.Windows.Controls;

namespace FileManager.Core.JobSteps.PluginTest;

[Plugin<JobStep>]
[PluginTypeName("ComputeFiles")]
[PluginDescription("Used to compute numerous files.")]
public class ComputeFilesJobStep : JobStep {
    public override void Execute(IServiceProvider serviceProvider) {
        
    }

    public override Task ExecuteAsync(IServiceProvider serviceProvider) {
        return Task.CompletedTask;
    }

    public override UserControl? GetJobStepView(bool createDataContext) {
        return null;
    }

    public override ImmutableResultCollection Validate(IServiceProvider serviceProvider) {
        return ImmutableResultCollection.Ok();
    }

    public override Task<ImmutableResultCollection> ValidateAsync(IServiceProvider serviceProvider) {
        return Task.FromResult(ImmutableResultCollection.Ok());
    }
}
