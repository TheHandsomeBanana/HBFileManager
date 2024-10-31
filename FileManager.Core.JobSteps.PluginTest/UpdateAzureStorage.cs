using FileManager.Domain.JobSteps;
using HBLibrary.DataStructures;
using HBLibrary.Interface.Plugins.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;

namespace FileManager.Core.JobSteps.PluginTest;

[Plugin<JobStep>]
[PluginTypeName("Update Azure Storage")]
[PluginDescription("Used to update a specified Azure storage with many adaptions and whatever lorem ipsum")]
public class UpdateAzureStorage : JobStep {
    public override void Execute(IUnityContainer container) {
        throw new NotImplementedException();
    }

    public override Task ExecuteAsync(IUnityContainer container) {
        throw new NotImplementedException();
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
