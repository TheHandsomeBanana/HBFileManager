using FileManager.Core.Jobs.ViewModels;
using FileManager.Core.Jobs.Views;
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
    #endregion

    #region Logic
    private int sourceItemLength = 0;
    private int processedFileCount = 0;
    private int skippedFileCount = 0;
    private int processedLogIndex = -1;
    private int skippedLogIndex = -1;
    private int progressBarLogIndex = -1;
    private int copyLogIndex = -1;
    private const int ProgressBarLength = 64;
    private const char ProgressBarChar = '=';
    private const char ProgressBarPlaceholderChar = '.';

    
    public override void Execute(IUnityContainer container) {
        IFileEntryService fileEntryService = container.Resolve<IFileEntryService>();
        IExtendedLogger logger = container.Resolve<IExtendedLogger>();

        PrepareLogs(logger);

        foreach (Entry sourceEntry in SourceItems) {
            foreach (Entry destinationEntry in DestinationItems) {
                switch (sourceEntry.Type) {
                    case EntryBrowseType.File:
                        if (ShouldCopy(sourceEntry.Path)) {
                            logger.RewriteIndexed(copyLogIndex, $"FILE::Copying {sourceEntry.Path} to {destinationEntry.Path}.");
                            fileEntryService.CopyFile(sourceEntry.Path, Path.Combine(destinationEntry.Path, Path.GetFileName(sourceEntry.Path)), CopyConflictAction.OverwriteModifiedOnly);
                            logger.RewriteIndexed(copyLogIndex, $"FILE::Copied {sourceEntry.Path} to {destinationEntry.Path}.");
                            LogProcessedFile(logger);
                        }
                        else {
                            LogSkippedFile(logger);
                        }
                        break;
                    case EntryBrowseType.Directory:
                        logger.RewriteIndexed(copyLogIndex, $"DIR::Copying {sourceEntry.Path} contents to {destinationEntry.Path}.");
                        CopyDirectory(fileEntryService, logger, sourceEntry.Path, destinationEntry.Path);
                        logger.RewriteIndexed(copyLogIndex, $"DIR::Copied {sourceEntry.Path} contents to {destinationEntry.Path}.");
                        break;
                }
            }
        }

        Reset();
    }

    public override async Task ExecuteAsync(IUnityContainer container) {
        IFileEntryService fileEntryService = container.Resolve<IFileEntryService>();
        IExtendedLogger logger = container.Resolve<IExtendedLogger>();

        PrepareLogs(logger);

        foreach (Entry sourceEntry in SourceItems) {
            foreach (Entry destinationEntry in DestinationItems) {
                switch (sourceEntry.Type) {
                    case EntryBrowseType.File:
                        if (ShouldCopy(sourceEntry.Path)) {
                            logger.RewriteIndexed(copyLogIndex, $"FILE::Copying {sourceEntry.Path} to {destinationEntry.Path}.");
                            await fileEntryService.CopyFileAsync(sourceEntry.Path, Path.Combine(destinationEntry.Path, Path.GetFileName(sourceEntry.Path)), CopyConflictAction.OverwriteModifiedOnly);
                            logger.RewriteIndexed(copyLogIndex, $"FILE::Copied {sourceEntry.Path} to {destinationEntry}.");

                            LogProcessedFile(logger);
                        }
                        else {
                            LogSkippedFile(logger);
                        }
                        break;
                    case EntryBrowseType.Directory:
                        logger.RewriteIndexed(copyLogIndex, $"DIR::Copying {sourceEntry.Path} contents to {destinationEntry.Path}.");
                        await CopyDirectoryAsync(fileEntryService, logger, sourceEntry.Path, destinationEntry.Path);
                        logger.RewriteIndexed(copyLogIndex, $"DIR::Copied {sourceEntry.Path} contents to {destinationEntry.Path}.");
                        break;
                }
            }
        }

        Reset();
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

    private void CopyDirectory(IFileEntryService fileEntryService, IExtendedLogger logger, string sourceDirectory, string destinationDirectory) {
        string? directoryName = Path.GetFileName(sourceDirectory); // Returns the name of the directory
        if (directoryName is null) {
            throw new InvalidOperationException($"Directory path {directoryName} invalid");
        }

        string nestedDirectory = Path.Combine(destinationDirectory, directoryName);
        Directory.CreateDirectory(nestedDirectory);

        foreach (string file in Directory.EnumerateFiles(sourceDirectory)) {
            if (ShouldCopy(file)) {
                string destinationFilePath = Path.Combine(nestedDirectory, Path.GetFileName(file));
                logger.RewriteIndexed(copyLogIndex, $"FILE::Copying {file} to {destinationDirectory}.");
                fileEntryService.CopyFile(file, destinationFilePath, CopyConflictAction.OverwriteModifiedOnly);

                LogProcessedFile(logger);
            }
            else {
                LogSkippedFile(logger);
            }
        }

        foreach (string directory in Directory.EnumerateDirectories(sourceDirectory)) {
            logger.RewriteIndexed(copyLogIndex, $"DIR::Copying {directory} contents to {destinationDirectory}.");
            CopyDirectory(fileEntryService, logger, directory, nestedDirectory);
            logger.RewriteIndexed(copyLogIndex, $"DIR::Copied {directory} contents to {destinationDirectory}.");
        }
    }

    private async Task CopyDirectoryAsync(IFileEntryService fileEntryService, IExtendedLogger logger, string sourceDirectory, string destinationDirectory) {
        string? directoryName = Path.GetFileName(sourceDirectory); // Returns the name of the directory
        if (directoryName is null) {
            throw new InvalidOperationException($"Directory path {directoryName} invalid");
        }

        string nestedDirectory = Path.Combine(destinationDirectory, directoryName);
        Directory.CreateDirectory(nestedDirectory);

        List<Task> copyTasks = [];
        foreach (string file in Directory.EnumerateFiles(sourceDirectory)) {
            if (ShouldCopy(file)) {
                await IOAsyncLimiter.FileSemaphore.WaitAsync();
                copyTasks.Add(Task.Run(async () => {
                    try {
                        string destinationFilePath = Path.Combine(nestedDirectory, Path.GetFileName(file));
                        logger.RewriteIndexed(copyLogIndex, $"FILE::ASYNC Copying {file} to {destinationDirectory}.");
                        await fileEntryService.CopyFileAsync(file, destinationFilePath, CopyConflictAction.OverwriteAll);
                        LogProcessedFile(logger);
                    }
                    finally {
                        IOAsyncLimiter.FileSemaphore.Release();
                    }
                }));
            }
            else {
                LogSkippedFile(logger);
            }
        }

        await Task.WhenAll(copyTasks);


        foreach (string directory in Directory.GetDirectories(sourceDirectory)) {
            logger.RewriteIndexed(copyLogIndex, $"DIR::Copying {directory} contents to {destinationDirectory}.");
            await CopyDirectoryAsync(fileEntryService, logger, directory, nestedDirectory);
            logger.RewriteIndexed(copyLogIndex, $"DIR::Copied {directory} contents to {destinationDirectory}.");
        }
    }


    private bool ShouldCopy(string file) {
        if (ModifiedOnly && TimeDifference is not null) {
            DateTime sourceLastWriteTime = File.GetLastWriteTimeUtc(file);
            string? destinationFile = FindDestinationFromSourceFile(file);

            if (destinationFile is null) {
                return true;
            }

            DateTime destinationLastWriteTime = File.GetLastWriteTimeUtc(destinationFile);
            return sourceLastWriteTime - destinationLastWriteTime > TimeDifference;
        }

        return true;
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

    private void PrepareLogs(IExtendedLogger logger) {
        sourceItemLength = SourceItems.SelectMany(e => {
            return e.Type switch {
                EntryBrowseType.File => [e.Path],
                EntryBrowseType.Directory => Directory.EnumerateFiles(e.Path, "*", SearchOption.AllDirectories),
                _ => [],
            };
        }).Count();


        logger.Info($"Files to process: {sourceItemLength}");
        processedLogIndex = logger.IndexedInfo($"Files processed: {processedFileCount}");
        skippedLogIndex = logger.IndexedInfo($"Files skipped: {skippedFileCount}");
        progressBarLogIndex = logger.IndexedInfo(GenerateProgressBar(0, sourceItemLength));
        copyLogIndex = logger.IndexedInfo($"Copying started");
    }

    private void LogProcessedFile(IExtendedLogger logger) {
        processedFileCount++;
        logger.RewriteIndexed(processedLogIndex, $"Files processed: {processedFileCount}");

        string progressBar = GenerateProgressBar(processedFileCount + skippedFileCount, sourceItemLength);
        logger.RewriteIndexed(progressBarLogIndex, progressBar);
    }

    private void LogSkippedFile(IExtendedLogger logger) {
        skippedFileCount++;
        logger.RewriteIndexed(skippedLogIndex, $"Files processed: {skippedFileCount}");

        string progressBar = GenerateProgressBar(processedFileCount + skippedFileCount, sourceItemLength);
        logger.RewriteIndexed(progressBarLogIndex, progressBar);
    }

    private static string GenerateProgressBar(int completed, int total) {
        if (total == 0) {
            return $"[{new string(ProgressBarPlaceholderChar, ProgressBarLength)}] 0%";
        }

        double progress = (double)completed / total;
        int completedChars = (int)(ProgressBarLength * progress);
        int remainingChars = ProgressBarLength - completedChars;

        return $"[{new string(ProgressBarChar, completedChars)}{new string(ProgressBarPlaceholderChar, remainingChars)}] {Math.Round(progress * 100)}%";
    }

    private void Reset() {
        sourceItemLength = 0;
        processedFileCount = 0;
        skippedFileCount = 0;
        processedLogIndex = -1;
        skippedLogIndex = -1;
        progressBarLogIndex = -1;
        copyLogIndex = -1;
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
