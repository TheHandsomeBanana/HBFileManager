using FileManager.Core.JobSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Job;
public class Job {
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsEnabled { get; set; }
    public bool CanRun { get; set; }
    public TimeOnly? ScheduledAt { get; set; }
    public bool OnDemand { get; set; }
    public bool Scheduled { get; set; }
    public string? Description { get; set; }
    public List<JobStep> Steps { get; set; } = [];
}
