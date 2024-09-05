using FileManager.Core.JobSteps.Attributes;
using FileManager.Core.JobSteps.ViewModels;
using FileManager.Core.JobSteps.Views;
using HBLibrary.Common.Extensions;
using HBLibrary.Services.IO;
using HBLibrary.Services.Logging;
using HBLibrary.Wpf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;

namespace FileManager.Core.JobSteps.Models;
[JobStepName("Copy")]
public class CopyStep : IJobStep, IAsyncJobStep {
    #region Model
    public string Name { get; set; } = "";
    public List<Entry> SourceItems { get; set; } = [];
    public List<Entry> DestinationItems { get; set; } = [];
    public bool ModifiedOnly { get; set; }
    public TimeSpan? TimeDifference { get; set; }
    public string? TimeDifferenceText { get; set; }
    public TimeUnit? TimeDifferenceUnit { get; set; }
    public bool UseAsyncCopy { get; set; }
    public int MaxConcurrency { get; set; }
    #endregion

    #region Logic
    // Cache known destinations for faster lookup
    private readonly HashSet<string> knownDestinations = [];

    public void Execute(IServiceProvider serviceProvider) {
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

    public async Task ExecuteAsync(IServiceProvider serviceProvider) {
        IAsyncLogger logger = (IAsyncLogger)serviceProvider.GetService(typeof(IAsyncLogger))!;
        IFileEntryService fileEntryService = (IFileEntryService)serviceProvider.GetService(typeof(IFileEntryService))!;

        IEnumerable<string> filteredSource = GetFilteredSourceItems();
        SemaphoreSlim semaphore = new SemaphoreSlim(MaxConcurrency);
        List<Task> copyTasks = [];

        foreach (string sourceFile in filteredSource) {
            string filename = Path.GetFileName(sourceFile);

            foreach (string destination in DestinationItems.Select(e => e.Path)) {

                await semaphore.WaitAsync();
                copyTasks.Add(Task.Run(async () =>
                {
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

    public HBLibrary.Common.Results.ValidationResult Validate(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public async Task<HBLibrary.Common.Results.ValidationResult> ValidateAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }


    // Concat provided files with files from provided directories
    private IEnumerable<string> GetFilteredSourceItems() {
        // Concat provided files with files from provided directories
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
        if (knownDestinations.TryGetValue(filepath, out string? knownDestination)) {
            return knownDestination;
        }

        foreach (Entry destination in DestinationItems) {
            string filename = Path.GetFileName(filepath);

            string possibleDestinationFile = Path.Combine(destination.Path, filename);

            if (File.Exists(possibleDestinationFile)) {
                knownDestinations.Add(possibleDestinationFile);
                return possibleDestinationFile;
            }
        }

        return null;
    }
    #endregion

    public UserControl? GetJobStepView() {
        CopyStepView copyStepView = new CopyStepView();
        copyStepView.DataContext = new CopyStepContext(this);
        return copyStepView;
    }
}

public class Entry {
    public required EntryType Type { get; set; }
    public required string Path { get; set; }
}
