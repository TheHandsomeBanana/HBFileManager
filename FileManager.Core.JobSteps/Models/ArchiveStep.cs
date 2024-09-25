using FileManager.Core.JobSteps.Attributes;
using FileManager.Core.JobSteps.ViewModels;
using HBLibrary.Common;
using HBLibrary.Common.Plugins.Attributes;
using HBLibrary.Wpf.ViewModels;

namespace FileManager.Core.JobSteps.Models;

[Plugin<JobStep>]
[PluginTypeName("Archive")]
public class ArchiveStep : JobStep {

    public override void Execute(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public override Task ExecuteAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public override System.Windows.Controls.UserControl? GetJobStepView() {
        return null;
    }

    public override ViewModelBase? GetJobStepDataContext() {
        return null;
    }

    public override ImmutableResultCollection Validate(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public override Task<ImmutableResultCollection> ValidateAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }
}
