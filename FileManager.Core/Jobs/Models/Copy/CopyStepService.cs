using HBLibrary.Core;
using HBLibrary.Core.Limiter;
using HBLibrary.Interface.IO;
using HBLibrary.Wpf.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Jobs.Models.Copy;
public class CopyStepService {
    private readonly IExtendedLogger logger;
    private readonly IFileEntryService fileEntryService;
    private readonly ConsoleProgressBar progressBar;
    private readonly List<Entry> sourceItems;
    private readonly List<Entry> destinationItems;
    private readonly bool modifiedOnly;
    private readonly TimeSpan? timeDifference;

    internal CopyStepLogData CopyStepLogData { get; }


    public CopyStepService(IExtendedLogger logger, IFileEntryService fileEntryService, List<Entry> sourceItems, List<Entry> destinationItems, bool modifiedOnly, TimeSpan? timeDifference) {
        this.logger = logger;
        this.fileEntryService = fileEntryService;
        this.sourceItems = sourceItems;
        this.destinationItems = destinationItems;
        this.modifiedOnly = modifiedOnly;
        this.timeDifference = timeDifference;

        int sourceItemLength = sourceItems.SelectMany(e => {
            return e.Type switch {
                EntryBrowseType.File => [e.Path],
                EntryBrowseType.Directory => Directory.EnumerateFiles(e.Path, "*", SearchOption.AllDirectories),
                _ => [],
            };
        }).Count();

        CopyStepLogData = new CopyStepLogData(sourceItemLength);

        progressBar = ProgressBars.CreateDefaultConsoleProgressBar(sourceItemLength);

        CopyStepLogData.Init(logger, progressBar);
    }

    public void LogProcessedFile() {
        CopyStepLogData.LogProcessedFile(logger, progressBar);
    }

    public void LogSkippedFile() {
        CopyStepLogData.LogSkippedFile(logger, progressBar);
    }


    public void Copy(string source, string destination) {
        logger.RewriteIndexed(CopyStepLogData.CopyLogIndex, $"FILE::Copying {source} to {destination}.");
        fileEntryService.CopyFile(source, Path.Combine(destination, Path.GetFileName(source)), CopyConflictAction.OverwriteModifiedOnly);
        logger.RewriteIndexed(CopyStepLogData.CopyLogIndex, $"FILE::Copied {source} to {destination}.");
    }

    public async Task CopyAsync(string source, string destination) {
        logger.RewriteIndexed(CopyStepLogData.CopyLogIndex, $"FILE::Copying {source} to {destination}.");
        await fileEntryService.CopyFileAsync(source, Path.Combine(destination, Path.GetFileName(source)), CopyConflictAction.OverwriteModifiedOnly);
        logger.RewriteIndexed(CopyStepLogData.CopyLogIndex, $"FILE::Copied {source} to {destination}.");
    }

    public void CopyDirectory(string sourceDirectory, string destinationDirectory) {
        logger.RewriteIndexed(CopyStepLogData.CopyLogIndex, $"DIR::Copying {sourceDirectory} contents to {destinationDirectory}.");

        string? directoryName = Path.GetFileName(sourceDirectory); // Returns the name of the directory
        if (directoryName is null) {
            throw new InvalidOperationException($"Directory path {directoryName} invalid");
        }

        string nestedDirectory = Path.Combine(destinationDirectory, directoryName);
        Directory.CreateDirectory(nestedDirectory);

        foreach (string file in Directory.EnumerateFiles(sourceDirectory)) {
            if (ShouldCopy(file)) {
                string destinationFilePath = Path.Combine(nestedDirectory, Path.GetFileName(file));
                logger.RewriteIndexed(CopyStepLogData.CopyLogIndex, $"FILE::Copying {file} to {destinationDirectory}.");
                fileEntryService.CopyFile(file, destinationFilePath, CopyConflictAction.OverwriteModifiedOnly);

                LogProcessedFile();
            }
            else {
                LogSkippedFile();
            }
        }

        foreach (string directory in Directory.EnumerateDirectories(sourceDirectory)) {
            CopyDirectory(directory, nestedDirectory);
        }

        logger.RewriteIndexed(CopyStepLogData.CopyLogIndex, $"DIR::Copied {sourceDirectory} contents to {destinationDirectory}.");
    }

    public async Task CopyDirectoryAsync(string sourceDirectory, string destinationDirectory) {
        logger.RewriteIndexed(CopyStepLogData.CopyLogIndex, $"DIR::Copying {sourceDirectory} contents to {destinationDirectory}.");

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
                        logger.RewriteIndexed(CopyStepLogData.CopyLogIndex, $"FILE::ASYNC Copying {file} to {destinationDirectory}.");
                        await fileEntryService.CopyFileAsync(file, destinationFilePath, CopyConflictAction.OverwriteAll);
                        LogProcessedFile();
                    }
                    finally {
                        IOAsyncLimiter.FileSemaphore.Release();
                    }
                }));
            }
            else {
                LogSkippedFile();
            }
        }

        await Task.WhenAll(copyTasks);


        foreach (string directory in Directory.GetDirectories(sourceDirectory)) {
            await CopyDirectoryAsync(directory, nestedDirectory);
        }

        logger.RewriteIndexed(CopyStepLogData.CopyLogIndex, $"DIR::Copied {sourceDirectory} contents to {destinationDirectory}.");
    }

    public bool ShouldCopy(string source) {
        if (modifiedOnly && timeDifference is not null) {
            DateTime sourceLastWriteTime = File.GetLastWriteTimeUtc(source);
            string? destinationFile = FindDestinationFromSourceFile(source);

            if (destinationFile is null) {
                return true;
            }

            DateTime destinationLastWriteTime = File.GetLastWriteTimeUtc(destinationFile);
            return sourceLastWriteTime - destinationLastWriteTime > timeDifference;
        }

        return true;
    }

    private string? FindDestinationFromSourceFile(string filepath) {
        string filename = Path.GetFileName(filepath);

        foreach (Entry destination in destinationItems) {
            string possibleDestinationFile = Path.Combine(destination.Path, filename);

            if (File.Exists(possibleDestinationFile)) {
                return possibleDestinationFile;
            }
        }

        return null;
    }


}
