using FileManager.Core.JobSteps;
using HBLibrary.DataStructures;
using HBLibrary.Interface.Core.ChangeTracker;
using HBLibrary.Wpf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Resources.Extensions;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Job;
public sealed class Job : TrackableModel, IDisposable {
    public Guid Id { get; set; }
    private string name = "";
    public string Name {
        get => name;
        set {
            name = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool isEnabled;
    public bool IsEnabled {
        get => isEnabled;
        set {
            isEnabled = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool canRun;
    public bool CanRun {
        get => canRun;
        set {
            canRun = value;
            NotifyTrackableChanged(value);
        }
    }

    private TimeOnly? scheduledAt;
    public TimeOnly? ScheduledAt {
        get => scheduledAt;
        set {
            scheduledAt = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool onDemand;
    public bool OnDemand {
        get => onDemand;
        set {
            onDemand = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool scheduled;
    public bool Scheduled {
        get => scheduled;
        set {
            scheduled = value;
            NotifyTrackableChanged(value);
        }
    }

    private string? description;
    public string? Description {
        get => description;
        set {
            description = value;
            NotifyTrackableChanged(value, nameof(Description));
        }
    }

    private TrackableList<JobStep> steps;
    public TrackableList<JobStep> Steps {
        get => steps;
        set {
            steps.TrackableChanged -= Steps_NotifyTrackableChanged;
            steps = value;
            steps.TrackableChanged += Steps_NotifyTrackableChanged;
            NotifyTrackableChanged(value);
        }
    }

    public Job() {
        steps = [];
        steps.TrackableChanged += Steps_NotifyTrackableChanged;
    }

    private void Steps_NotifyTrackableChanged(object? sender, TrackedChanges e) {
        NotifyNestedTrackableChanged(sender, e.Value, nameof(JobStep), e.Name);
    }

    public void Dispose() {
        steps.TrackableChanged -= Steps_NotifyTrackableChanged;
    }
}
