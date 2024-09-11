using FileManager.Core.JobSteps;

namespace FileManager.UI.Models.JobModels;
public class JobItemModel {
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public TimeOnly? ScheduledAt { get; set; }
    public bool OnDemand { get; set; }
    public bool Scheduled { get; set; }
    public string? Description { get; set; }
    public Dictionary<Guid, IJobStep> Steps { get; set; } = [];
}
