using FileManager.Core.Jobs.ViewModels;
using FileManager.Core.Jobs.Views;
using FileManager.Domain.JobSteps;
using HBLibrary.Common;
using HBLibrary.DataStructures;
using HBLibrary.Interface.IO;
using HBLibrary.Interface.Logging;
using HBLibrary.Interface.Plugins.Attributes;
using HBLibrary.Wpf.Models;
using HBLibrary.Wpf.ViewModels;
using System.IO;
using Unity;

namespace FileManager.Core.Jobs.Models;

[Plugin<JobStep>]
[PluginTypeName("Copy")]
[PluginDescription("Copy files and directories from the source definition to destination definition.")]
public class CopyStep : JobStep {
    #region Model
    public List<Entry> SourceItems { get; set; } = [];
    public List<Entry> DestinationItems { get; set; } = [];
    public bool ModifiedOnly { get; set; }
    public TimeSpan? TimeDifference { get; set; }
    public string? TimeDifferenceText { get; set; }
    public TimeUnit? TimeDifferenceUnit { get; set; }
    public int MaxConcurrency { get; set; } = 6;
    #endregion

    #region Logic
    public override void Execute(IUnityContainer container) {
        IFileEntryService fileEntryService = container.Resolve<IFileEntryService>();

        IEnumerable<string> filteredSource = GetFilteredSourceItems();

        foreach (string sourceFile in filteredSource) {
            string filename = Path.GetFileName(sourceFile);

            foreach (string destination in DestinationItems.Select(e => e.Path)) {
                fileEntryService.CopyFile(sourceFile, Path.Combine(destination, filename), CopyConflictAction.OverwriteModifiedOnly);
            }
        }
    }

    public override async Task ExecuteAsync(IUnityContainer container) {
        IFileEntryService fileEntryService = container.Resolve<IFileEntryService>();

        IEnumerable<string> filteredSource = GetFilteredSourceItems();

        int limitedConcurrency = Math.Min(MaxConcurrency, DestinationItems.Count);


        SemaphoreSlim semaphore = new SemaphoreSlim(limitedConcurrency);
        List<Task> copyTasks = [];

        foreach (string sourceFile in filteredSource) {
            string filename = Path.GetFileName(sourceFile);

            foreach (string destination in DestinationItems.Select(e => e.Path)) {
                await semaphore.WaitAsync();
                copyTasks.Add(Task.Run(async () => {
                    try {
                        await fileEntryService.CopyFileAsync(sourceFile, Path.Combine(destination, filename), CopyConflictAction.OverwriteModifiedOnly);
                    }
                    finally {
                        semaphore.Release();
                    }
                }));
            }
        }

        await Task.WhenAll(copyTasks);
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

        foreach (Entry destination in DestinationItems) {
            if (!Directory.Exists(destination.Path)) {
                string error = $"Destination directory '{destination.Path}' does not exist.";
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

        foreach (Entry destination in DestinationItems) {

            if (!Directory.Exists(destination.Path)) {
                string error = $"Destination directory '{destination.Path}' does not exist.";
                results.Add(Result.Fail(error));
            }
        }


        return results.Count != 0
            ? Task.FromResult(results.ToImmutableResultCollection())
            : Task.FromResult(ImmutableResultCollection.Ok());
    }


    // Concat provided files with files from provided directories
    private IEnumerable<string> GetFilteredSourceItems() {
        IEnumerable<string> fullSourceItems = SourceItems
            .SelectMany(e => {
                return e.Type switch {
                    EntryBrowseType.File => [e.Path],
                    EntryBrowseType.Directory => Directory.EnumerateFiles(e.Path, "*", SearchOption.AllDirectories),
                    _ => [],
                };
            });

        IEnumerable<string> filteredSource = fullSourceItems;

        if (ModifiedOnly && TimeDifference is not null) {
            // Only include files that are modified based on time difference
            filteredSource = fullSourceItems.Where(e => {
                DateTime sourceLastWriteTime = File.GetLastWriteTimeUtc(e);
                string? destinationFile = FindDestinationFromSourceFile(e);

                if (destinationFile is null) {
                    return true;
                }

                DateTime destinationLastWriteTime = File.GetLastWriteTimeUtc(destinationFile);
                return sourceLastWriteTime - destinationLastWriteTime > TimeDifference;
            });
        }

        return filteredSource;
    }

    private string? FindDestinationFromSourceFile(string filepath) {
        string filename = Path.GetFileName(filepath);

        foreach (Entry destination in DestinationItems) {
            string possibleDestinationFile = Path.Combine(destination.Path, filename);

            if (File.Exists(possibleDestinationFile)) {
                return possibleDestinationFile;
            }
        }

        return null;
    }
    #endregion

    public override System.Windows.Controls.UserControl? GetJobStepView() {
        return new CopyStepView();
    }

    public override IJobStepContext? GetJobStepDataContext() {
        return new CopyStepViewModel(this);
    }
}

public class Entry {
    public required EntryBrowseType Type { get; set; }
    public required string Path { get; set; }
}
