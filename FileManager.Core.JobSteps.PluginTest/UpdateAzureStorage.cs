using HBLibrary.Common;
using HBLibrary.Common.Plugins.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileManager.Core.JobSteps.PluginTest;

[Plugin<JobStep>]
[PluginTypeName("Update Azure Storage")]
[PluginDescription("Used to update a specified Azure storage with many adaptions and whatever lorem ipsum")]
public class UpdateAzureStorage : JobStep {
    public override void Execute(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public override Task ExecuteAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public override UserControl? GetJobStepView(bool createDataContext) {
        return null;
    }

    public override ImmutableResultCollection Validate(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public override Task<ImmutableResultCollection> ValidateAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }
}
