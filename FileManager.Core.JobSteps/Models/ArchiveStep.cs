using FileManager.Core.JobSteps.Attributes;
using HBLibrary.Common;
using HBLibrary.Common.Plugins.Attributes;

namespace FileManager.Core.JobSteps.Models;

[Plugin(typeof(IJobStep))]
[Plugin<IJobStep>]
[PluginTypeName("Archive")]
public class ArchiveStep : IJobStep {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public bool IsAsync { get; set; }

    public void Execute(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public Task ExecuteAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public System.Windows.Controls.UserControl? GetJobStepView() {
        return null;
    }

    public ImmutableResultCollection Validate(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public Task<ImmutableResultCollection> ValidateAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }
}
