using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HBLibrary.Wpf.Logging;
using FileManager.Core.Jobs.Models.Copy;

namespace FileManager.Core.Jobs.Models.Zip;
public class ZipArchiveStepService {
    private readonly IExtendedLogger logger;
    private readonly Entry[] sourceEntries;
    private readonly string destinationEntry;
    public ZipArchiveStepService(IExtendedLogger logger, Entry[] sourceEntries, string destination) {
        this.logger = logger;
        this.sourceEntries = sourceEntries;
        this.destinationEntry = destination;
    }

    public void CreateArchive() {
        using FileStream memoryStream = new FileStream(destinationEntry, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        using ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create);

        foreach (Entry entry in sourceEntries) {
            switch (entry.Type) {
                case EntryBrowseType.File:
                    ZipArchiveEntry archiveEntry = archive.CreateEntry(Path.GetFileName(entry.Path));
                    using (Stream entryStream = archiveEntry.Open()) {
                        using (FileStream fileStream = new FileStream(entry.Path, FileMode.Open, FileAccess.Read)) {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                    break;
                case EntryBrowseType.Directory:
                    AddDirectoryToZip(entry.Path, archive);
                    break;
            }
        }
    }

    public async Task CreateArchiveAsync() {
        using FileStream memoryStream = new FileStream(destinationEntry, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        using ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create);

        foreach (Entry entry in sourceEntries) {
            switch (entry.Type) {
                case EntryBrowseType.File:
                    ZipArchiveEntry archiveEntry = archive.CreateEntry(Path.GetFileName(entry.Path));
                    using (Stream entryStream = archiveEntry.Open()) {
                        using (FileStream fileStream = new FileStream(entry.Path, FileMode.Open, FileAccess.Read)) {
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }
                    break;
                case EntryBrowseType.Directory:
                    await AddDirectoryToZipAsync(entry.Path, archive);
                    break;
            }
        }
    }

    private void AddDirectoryToZip(string directoryPath, ZipArchive archive, string parentFolder = "") {
        string folderName = Path.GetFileName(directoryPath);
        string currentFolder = string.IsNullOrEmpty(parentFolder) ? folderName : Path.Combine(parentFolder, folderName);

        // Add a directory entry (trailing slash ensures it's treated as a directory in ZIP)
        archive.CreateEntry($"{currentFolder}/");

        foreach (string file in Directory.GetFiles(directoryPath)) {
            string fileName = Path.GetFileName(file);
            string entryPath = Path.Combine(currentFolder, fileName);

            ZipArchiveEntry entry = archive.CreateEntry(entryPath);
            using (Stream entryStream = entry.Open()) {
                using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read)) {
                    fileStream.CopyTo(entryStream);
                }
            }
        }

        foreach (string subdirectory in Directory.GetDirectories(directoryPath)) {
            AddDirectoryToZip(subdirectory, archive, currentFolder);
        }
    }

    private async Task AddDirectoryToZipAsync(string directoryPath, ZipArchive zipArchive, string parentFolder = "") {
        string folderName = Path.GetFileName(directoryPath);
        string currentFolder = string.IsNullOrEmpty(parentFolder) ? folderName : Path.Combine(parentFolder, folderName);

        // Add a directory entry (trailing slash ensures it's treated as a directory in ZIP)
        zipArchive.CreateEntry($"{currentFolder}/");

        foreach (string file in Directory.GetFiles(directoryPath)) {
            string fileName = Path.GetFileName(file);
            string entryPath = Path.Combine(currentFolder, fileName);

            ZipArchiveEntry entry = zipArchive.CreateEntry(entryPath);
            using Stream entryStream = entry.Open();
            using FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

            await fileStream.CopyToAsync(entryStream);
        }

        // Recursively add subdirectories
        foreach (string subdirectory in Directory.GetDirectories(directoryPath)) {
            await AddDirectoryToZipAsync(subdirectory, zipArchive, currentFolder);
        }
    }
}
