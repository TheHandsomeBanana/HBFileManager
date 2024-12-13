using FileManager.Core.Jobs.Models.Copy;
using FileManager.Core.Jobs.ViewModels;
using FileManager.Core.Jobs.Views;
using FileManager.Core.JobSteps;
using FileManager.Domain.JobSteps;
using HBLibrary.Common;
using HBLibrary.DataStructures;
using HBLibrary.Interface.IO;
using HBLibrary.Interface.Plugins.Attributes;
using HBLibrary.Wpf.Models;
using HBLibrary.Wpf.ViewModels;
using System.IO;
using Unity;
using System.IO.Compression;
using HBLibrary.Wpf.Logging;

namespace FileManager.Core.Jobs.Models.Zip;

[Plugin<JobStep>]
[PluginTypeName("ZipArchive")]
[PluginDescription("With this step you can create ZIP archives")]
public class ZipArchiveStep : JobStep {
    #region Model
    public TrackableList<Entry> SourceItems { get; set; } = [];
    public string? Destination { get; set; }
    #endregion

    public override void Execute(IUnityContainer container, CancellationToken stepCancellationToken = default, CancellationToken jobCancellationToken = default) {
        IExtendedLogger logger = container.Resolve<IExtendedLogger>();
        ZipArchiveStepService service = new ZipArchiveStepService(logger, [.. SourceItems], Destination!);
        service.CreateArchive();
    }

    public override async Task ExecuteAsync(IUnityContainer container, CancellationToken stepCancellationToken = default, CancellationToken jobCancellationToken = default) {
        IExtendedLogger logger = container.Resolve<IExtendedLogger>();
        ZipArchiveStepService service = new ZipArchiveStepService(logger, [.. SourceItems], Destination!);
        await service.CreateArchiveAsync();
    }

    public override System.Windows.Controls.UserControl? GetJobStepView() {
        return new ZipArchiveStepView();
    }

    public override IJobStepContext? GetJobStepDataContext() {
        return new ZipArchiveStepViewModel(this);
    }

    public override ImmutableResultCollection Validate(IUnityContainer container) {
        ResultCollection results = [];

        foreach (Entry source in SourceItems) {

            switch (source.Type) {
                case EntryBrowseType.File:
                    if (!File.Exists(source.Path)) {
                        string error = $"Source file '{source.Path}' does not exist.";
                        results.Add(Result.Fail(error));
                    }
                    break;
                case EntryBrowseType.Directory:
                    if (!Directory.Exists(source.Path)) {
                        string error = $"Source directory '{source.Path}' does not exist.";
                        results.Add(Result.Fail(error));
                    }
                    break;
            }
        }

        return results;
    }

    public override Task<ImmutableResultCollection> ValidateAsync(IUnityContainer container) {
        ResultCollection results = [];

        foreach (Entry source in SourceItems) {

            switch (source.Type) {
                case EntryBrowseType.File:
                    if (!File.Exists(source.Path)) {
                        results.Add(Result.Fail($"Source file '{source.Path}' does not exist."));
                    }
                    break;
                case EntryBrowseType.Directory:
                    if (!Directory.Exists(source.Path)) {
                        results.Add(Result.Fail($"Source directory '{source.Path}' does not exist."));
                    }
                    break;
            }
        }

        if (Destination is null) {
            results.Add(Result.Fail($"Target zip archive is not set."));
        }
        else {
            if (!PathValidator.ValidatePath(Destination)) {
                results.Add(Result.Fail($"Target zip archive path contains illegal characters"));
            }
        }

        return results.Count != 0
            ? Task.FromResult(results.ToImmutableResultCollection())
            : Task.FromResult(ImmutableResultCollection.Ok());
    }
}
