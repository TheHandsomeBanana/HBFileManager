using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Models.Job;
public class JobItemModel {
    public Guid Id { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public bool OnDemand { get; set; }
    public bool Scheduled { get; set; }
    public string? Description { get; set; }
    public List<JobItemStepModel> Steps { get; set; } = [];
}
