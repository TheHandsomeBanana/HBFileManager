﻿using FileManager.Core.JobSteps.Attributes;
using FileManager.Core.JobSteps.ViewModels;
using FileManager.Core.JobSteps.Views;
using HBLibrary.Common.Results;
using HBLibrary.Services.IO;
using HBLibrary.Services.Logging;
using HBLibrary.Wpf.Models;
using System.IO;

namespace FileManager.Core.JobSteps.Models;
[JobStepType("Copy")]
[JobStepDescription("Copy files and directories from the source definition to destination definition.")]
public class CopyStep : IJobStep {
    #region Model
    public string Name { get; set; } = "";
    public List<Entry> SourceItems { get; set; } = [];
    public List<Entry> DestinationItems { get; set; } = [];
    public bool ModifiedOnly { get; set; }
    public TimeSpan? TimeDifference { get; set; }
    public string? TimeDifferenceText { get; set; }
    public TimeUnit? TimeDifferenceUnit { get; set; }
    public bool IsAsync { get; set; }
    public int MaxConcurrency { get; set; } = 6;
    public Guid Id { get; set; } = Guid.NewGuid();
    #endregion

    #region Logic
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

    public CollectionResult Validate(IServiceProvider serviceProvider) {
        ILogger logger = (ILogger)serviceProvider.GetService(typeof(ILogger))!;
        List<string> errors = [];

        foreach (Entry source in SourceItems) {
            logger.Info($"Validating source item '{source.Path}'");

            switch (source.Type) {
                case EntryType.File:
                    if (!File.Exists(source.Path)) {
                        string error = $"Source file '{source.Path}' does not exist.";
                        logger.Error(error);
                        errors.Add(error);
                    }
                    break;
                case EntryType.Directory:
                    if (!Directory.Exists(source.Path)) {
                        string error = $"Source directory '{source.Path}' does not exist.";
                        logger.Error(error);
                        errors.Add(error);
                    }
                    break;
            }
        }

        foreach (Entry destination in DestinationItems) {
            if (!Directory.Exists(destination.Path)) {
                string error = $"Destination directory '{destination.Path}' does not exist.";
                logger.Error(error);
                errors.Add(error);
            }
        }

        if (errors.Count == 0) {
            return CollectionResult.Ok();
        }

        return CollectionResult.Fail(errors);
    }

    public Task<CollectionResult> ValidateAsync(IServiceProvider serviceProvider) {
        IAsyncLogger logger = (IAsyncLogger)serviceProvider.GetService(typeof(IAsyncLogger))!;
        List<string> errors = [];

        foreach (Entry source in SourceItems) {
            logger.Info($"Validating source item '{source.Path}'");

            switch (source.Type) {
                case EntryType.File:
                    if (!File.Exists(source.Path)) {
                        string error = $"Source file '{source.Path}' does not exist.";
                        logger.Error(error);
                        errors.Add(error);
                    }
                    break;
                case EntryType.Directory:
                    if (!Directory.Exists(source.Path)) {
                        string error = $"Source directory '{source.Path}' does not exist.";
                        logger.Error(error);
                        errors.Add(error);
                    }
                    break;
            }
        }

        foreach (Entry destination in DestinationItems) {
            logger.Info($"Validating destination item '{destination.Path}'");

            if (!Directory.Exists(destination.Path)) {
                string error = $"Destination directory '{destination.Path}' does not exist.";
                logger.Error(error);
                errors.Add(error);
            }
        }

        if (errors.Count == 0) {
            return Task.FromResult(CollectionResult.Ok());
        }

        return Task.FromResult(CollectionResult.Fail(errors));
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

    public System.Windows.Controls.UserControl? GetJobStepView() {
        CopyStepView copyStepView = new CopyStepView();
        copyStepView.DataContext = new CopyStepViewModel(this);
        return copyStepView;
    }
}

public class Entry {
    public required EntryType Type { get; set; }
    public required string Path { get; set; }
}
