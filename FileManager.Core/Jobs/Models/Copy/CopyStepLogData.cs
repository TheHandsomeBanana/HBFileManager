using HBLibrary.Core;
using HBLibrary.Wpf.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileManager.Core.Jobs.Models.Copy;
internal class CopyStepLogData {
    internal int SourceItemLength { get; }
    internal int ProcessedFileCount { get; private set; } = 0;
    internal int SkippedFileCount { get; private set; } = 0;
    internal int ProcessedLogIndex { get; private set; } = -1;
    internal int SkippedLogIndex { get; private set; } = -1;
    internal int ProgressBarLogIndex { get; private set; } = -1;
    internal int CopyLogIndex { get; private set; } = -1;

    public CopyStepLogData(int sourceItemLength) {
        this.SourceItemLength = sourceItemLength;
    }

    public void Init(IExtendedLogger logger, ConsoleProgressBar progressBar) {
        logger.Info($"Files to process: {SourceItemLength}");
        ProcessedLogIndex = logger.IndexedInfo($"Files processed: {ProcessedFileCount}");
        SkippedLogIndex = logger.IndexedInfo($"Files skipped: {SkippedFileCount}");
        ProgressBarLogIndex = logger.IndexedInfo(progressBar.Generate(0));
        CopyLogIndex = logger.IndexedInfo($"Copying started");
    }

    public void LogProcessedFile(IExtendedLogger logger, ConsoleProgressBar progressBar) {
        ProcessedFileCount++;
        logger.RewriteIndexed(ProcessedLogIndex, $"Files processed: {ProcessedFileCount}");

        string progressBarString = progressBar.Generate(ProcessedFileCount + SkippedFileCount);
        logger.RewriteIndexed(ProgressBarLogIndex, progressBarString);
    }

    public void LogSkippedFile(IExtendedLogger logger, ConsoleProgressBar progressBar) {
        SkippedFileCount++;
        logger.RewriteIndexed(SkippedLogIndex, $"Files processed: {SkippedFileCount}");

        string progressBarString = progressBar.Generate(ProcessedFileCount + SkippedFileCount);
        logger.RewriteIndexed(ProgressBarLogIndex, progressBarString);
    }
}
