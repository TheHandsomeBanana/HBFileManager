using FileManager.UI.Models.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Models.JobModels.JobStepModels;
public class CopyStepModel : JobItemStepModel {
    public override StepType StepType => StepType.Copy;

    public string? Source { get; set; }
    public TargetType? SourceType { get; set; }
    public string? Destination { get; set; }
    public TargetType? DestinationType { get; set; }

    public bool ModifiedOnly { get; set; }
    public TimeSpan? TimeDifference { get; set; }
}
