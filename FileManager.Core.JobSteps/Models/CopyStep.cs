using FileManager.Core.JobSteps.Attributes;
using FileManager.Core.JobSteps.ViewModels;
using FileManager.Core.JobSteps.Views;
using HBLibrary.Common;
using HBLibrary.Common.Plugins.Attributes;
using HBLibrary.Services.IO;
using HBLibrary.Services.Logging;
using HBLibrary.Wpf.Models;
using System.IO;

namespace FileManager.Core.JobSteps.Models;

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
    public override void Execute(IServiceProvider serviceProvider) {
        ILogger logger = (ILogger)serviceProvider.GetService(typeof(ILogger))!;
        IFileEntryService fileEntryService = (IFileEntryService)serviceProvider.GetService(typeof(IFileEntryService))!;

        IEnumerable<string> filteredSource = GetFilteredSourceItems();

        foreach (string sourceFile in filteredSource) {
            string filename = Path.GetFileName(sourceFile);

            foreach (string destination in DestinationItems.Select(e => e.Path)) {
                fileEntryService.CopyFile(sourceFile, Path.Combine(destination, filename), CopyConflictAction.OverwriteModifiedOnly);
            }
        }
    }

    public override async Task ExecuteAsync(IServiceProvider serviceProvider) {
        IAsyncLogger logger = (IAsyncLogger)serviceProvider.GetService(typeof(IAsyncLogger))!;
        IFileEntryService fileEntryService = (IFileEntryService)serviceProvider.GetService(typeof(IFileEntryService))!;

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

    public override ImmutableResultCollection Validate(IServiceProvider serviceProvider) {
        ILogger logger = (ILogger)serviceProvider.GetService(typeof(ILogger))!;
        ResultCollection results = [];

        foreach (Entry source in SourceItems) {
            logger.Info($"Validating source item '{source.Path}'");

            switch (source.Type) {
                case EntryType.File:
                    if (!File.Exists(source.Path)) {
                        string error = $"Source file '{source.Path}' does not exist.";
                        logger.Error(error);
                        results.Add(Result.Fail(error));
                    }
                    break;
                case EntryType.Directory:
                    if (!Directory.Exists(source.Path)) {
                        string error = $"Source directory '{source.Path}' does not exist.";
                        logger.Error(error);
                        results.Add(Result.Fail(error));
                    }
                    break;
            }
        }

        foreach (Entry destination in DestinationItems) {
            if (!Directory.Exists(destination.Path)) {
                string error = $"Destination directory '{destination.Path}' does not exist.";
                logger.Error(error);
                results.Add(Result.Fail(error));
            }
        }

        return results;
    }

    public override Task<ImmutableResultCollection> ValidateAsync(IServiceProvider serviceProvider) {
        IAsyncLogger logger = (IAsyncLogger)serviceProvider.GetService(typeof(IAsyncLogger))!;
        ResultCollection results = [];

        foreach (Entry source in SourceItems) {
            logger.Info($"Validating source item '{source.Path}'");

            switch (source.Type) {
                case EntryType.File:
                    if (!File.Exists(source.Path)) {
                        string error = $"Source file '{source.Path}' does not exist.";
                        logger.Error(error);
                        results.Add(Result.Fail(error));
                    }
                    break;
                case EntryType.Directory:
                    if (!Directory.Exists(source.Path)) {
                        string error = $"Source directory '{source.Path}' does not exist.";
                        logger.Error(error);
                        results.Add(Result.Fail(error));
                    }
                    break;
            }
        }

        foreach (Entry destination in DestinationItems) {
            logger.Info($"Validating destination item '{destination.Path}'");

            if (!Directory.Exists(destination.Path)) {
                string error = $"Destination directory '{destination.Path}' does not exist.";
                logger.Error(error);
                results.Add(Result.Fail(error));
            }
        }

        
        return Task.FromResult(results.ToImmutableResultCollection());
    }


    // Concat provided files with files from provided directories
    private IEnumerable<string> GetFilteredSourceItems() {
        IEnumerable<string> fullSourceItems = SourceItems
            .SelectMany(e => {
                return e.Type switch {
                    EntryType.File => [e.Path],
                    EntryType.Directory => Directory.EnumerateFiles(e.Path, "*", SearchOption.AllDirectories),
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
        CopyStepView copyStepView = new CopyStepView();
        copyStepView.DataContext = new CopyStepViewModel(this);
        return copyStepView;
    }
}

public class Entry {
    public required EntryType Type { get; set; }
    public required string Path { get; set; }
}
