using FileManager.Core.Jobs.ViewModels;
using FileManager.Core.Jobs.Views;
using FileManager.Core.JobSteps;
using FileManager.Domain.JobSteps;
using HBLibrary.Common;
using HBLibrary.DataStructures;
using HBLibrary.Interface.Plugins.Attributes;
using HBLibrary.Wpf.Models;
using HBLibrary.Wpf.ViewModels;
using Unity;

namespace FileManager.Core.Jobs.Models;

[Plugin<JobStep>]
[PluginTypeName("ZipArchive")]
[PluginDescription("With this step you can create ZIP archives")]
public class ZipArchiveStep : JobStep {
    #region Model
    public List<Entry> SourceItems { get; set; } = [];
    public List<Entry> DestinationItems { get; set; } = [];
    public bool ModifiedOnly { get; set; }
    public TimeSpan? TimeDifference { get; set; }
    public string? TimeDifferenceText { get; set; }
    public TimeUnit? TimeDifferenceUnit { get; set; }
    public int MaxConcurrency { get; set; } = 6;
    #endregion

    public override void Execute(IUnityContainer container) {
        IExecutionStateHandler stateHandler = container.Resolve<IExecutionStateHandler>();
        stateHandler.WillCompleteWithWarnings();

        Thread.Sleep(5000);
    }

    public override async Task ExecuteAsync(IUnityContainer container) {
        await Task.Delay(4000);
    }

    public override System.Windows.Controls.UserControl? GetJobStepView() {
        return new ZipArchiveStepView();
    }

    public override IJobStepContext? GetJobStepDataContext() {
        return new ZipArchiveStepViewModel(this);
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
