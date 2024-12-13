using FileManager.Core.Jobs.ViewModels;
using FileManager.Core.Jobs.Views;
using FileManager.Core.JobSteps;
using FileManager.Domain.JobSteps;
using HBLibrary.Common;
using HBLibrary.Core.Limiter;
using HBLibrary.DataStructures;
using HBLibrary.Interface.IO;
using HBLibrary.Interface.Logging;
using HBLibrary.Interface.Plugins.Attributes;
using HBLibrary.Wpf.Logging;
using HBLibrary.Wpf.Logging.Statements;
using HBLibrary.Wpf.Models;
using HBLibrary.Wpf.ViewModels;
using System.Diagnostics;
using System.IO;
using Unity;

namespace FileManager.Core.Jobs.Models.Copy;

[Plugin<JobStep>]
[PluginTypeName("Copy")]
[PluginDescription("Copy files and directories from the source definition to destination definition.")]
public class CopyStep : JobStep {
    #region Model
    public List<Entry> SourceItems { get; set; } = [];
    public List<string> DestinationItems { get; set; } = [];
    public bool ModifiedOnly { get; set; }
    public TimeSpan? TimeDifference { get; set; }
    public string? TimeDifferenceText { get; set; }
    public TimeUnit? TimeDifferenceUnit { get; set; }
    #endregion

    #region Logic
    public override void Execute(IUnityContainer container, CancellationToken stepCancellationToken = default, CancellationToken jobCancellationToken = default) {
        IFileEntryService fileEntryService = container.Resolve<IFileEntryService>();
        IExtendedLogger logger = container.Resolve<IExtendedLogger>();
        IExecutionStateHandler executionStateHandler = container.Resolve<IExecutionStateHandler>();

        CopyStepService copyStepService = new CopyStepService(logger, fileEntryService, executionStateHandler, SourceItems, DestinationItems, ModifiedOnly, TimeDifference);

        foreach (Entry sourceEntry in SourceItems) {
            foreach (string destination in DestinationItems) {
                if (jobCancellationToken.IsCancellationRequested || stepCancellationToken.IsCancellationRequested) {
                    executionStateHandler.IsCanceled();
                    return;
                }

                switch (sourceEntry.Type) {
                    case EntryBrowseType.File:
                        if (copyStepService.ShouldCopy(sourceEntry.Path)) {
                            copyStepService.Copy(sourceEntry.Path, destination);
                            copyStepService.LogProcessedFile();
                        }
                        else {
                            copyStepService.LogSkippedFile();
                        }
                        break;
                    case EntryBrowseType.Directory:
                        copyStepService.CopyDirectory(sourceEntry.Path, destination, stepCancellationToken, jobCancellationToken);
                        break;
                }
            }
        }
    }

    public override async Task ExecuteAsync(IUnityContainer container, CancellationToken stepCancellationToken = default, CancellationToken jobCancellationToken = default) {
        IFileEntryService fileEntryService = container.Resolve<IFileEntryService>();
        IExtendedLogger logger = container.Resolve<IExtendedLogger>();
        IExecutionStateHandler executionStateHandler = container.Resolve<IExecutionStateHandler>();

        CopyStepService copyStepService = new CopyStepService(logger, fileEntryService, executionStateHandler, SourceItems, DestinationItems, ModifiedOnly, TimeDifference);

        foreach (Entry sourceEntry in SourceItems) {
            foreach (string destination in DestinationItems) {
                if (jobCancellationToken.IsCancellationRequested || stepCancellationToken.IsCancellationRequested) {
                    executionStateHandler.IsCanceled();
                    return;
                }

                switch (sourceEntry.Type) {
                    case EntryBrowseType.File:
                        if (copyStepService.ShouldCopy(sourceEntry.Path)) {
                            await copyStepService.CopyAsync(sourceEntry.Path, destination);
                            copyStepService.LogProcessedFile();
                        }
                        else {
                            copyStepService.LogSkippedFile();
                        }
                        break;
                    case EntryBrowseType.Directory:
                        await copyStepService.CopyDirectoryAsync(sourceEntry.Path, destination, stepCancellationToken, jobCancellationToken);
                        break;
                }
            }
        }
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

        foreach (string destination in DestinationItems) {
            if (!Directory.Exists(destination)) {
                string error = $"Destination directory '{destination}' does not exist.";
                results.Add(Result.Fail(error));
            }
        }

        return results.Count != 0
            ? results
            : ImmutableResultCollection.Ok();
    }

    public override Task<ImmutableResultCollection> ValidateAsync(IUnityContainer container) {
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

        foreach (string destination in DestinationItems) {

            if (!Directory.Exists(destination)) {
                string error = $"Destination directory '{destination}' does not exist.";
                results.Add(Result.Fail(error));
            }
        }


        return results.Count != 0
            ? Task.FromResult(results.ToImmutableResultCollection())
            : Task.FromResult(ImmutableResultCollection.Ok());
    }

    
    #endregion

    public override System.Windows.Controls.UserControl? GetJobStepView() {
        return new CopyStepView();
    }

    public override IJobStepContext? GetJobStepDataContext() {
        return new CopyStepViewModel(this);
    }
}

public class Entry : TrackableModel {
    public required EntryBrowseType Type { get; set; }

    private string path = "";
    public required string Path { 
        get => path; 
        set {
            path = value;
            NotifyTrackableChanged(value, nameof(Path));
        } 
    }
}
