using HBLibrary.Wpf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Models.JobModels.JobStepModels;
public class CopyStepModel : JobItemStepModel {
    public override StepType StepType => StepType.Copy;

    public List<FileEntryWrapper> SourceItems { get; set; } = [];
    public List<FileEntryWrapper> DestinationItems { get; set; } = [];



    public bool ModifiedOnly { get; set; }
    public TimeSpan? TimeDifference { get; set; }
    public string? TimeDifferenceText { get; set; }
    public TimeUnit? TimeDifferenceUnit { get; set; }
}

public class FileEntryWrapper {
    public FileEntryType Type { get; set; }
    public required string Path { get; set; }
}
