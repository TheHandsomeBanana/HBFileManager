using HBLibrary.Common;
using HBLibrary.DataStructures;
using HBLibrary.Wpf.Models;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;

namespace FileManager.Domain.JobSteps;

/// <summary>
/// Classes that implement this abstract class are responsible for executing a job step. They MUST have a parameterless constructor.
/// <br></br>
/// They should also serve as model for the corresponding view if used. 
/// <br></br>
/// If you want custom properties to participate in change tracking, 
/// ensure <see cref="TrackableModel.NotifyTrackableChanged(object?, string)"/> is called in the setter.
/// </summary>
public abstract class JobStep : TrackableModel, IJobStep {
    public Guid Id { get; set; } = Guid.NewGuid();

    private string name = "";
    public string Name {
        get => name;
        set {
            name = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool isAsync;
    public bool IsAsync {
        get => isAsync;
        set {
            isAsync = value;
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

    private bool isValid;
    public bool IsValid {
        get => isValid;
        set {
            isValid = value;
            NotifyTrackableChanged(value);
        }
    }

    private int executionOrder;
    public int ExecutionOrder {
        get => executionOrder;
        set {
            executionOrder = value;
            NotifyTrackableChanged(value);
        }
    }

    public abstract void Execute(IUnityContainer container, CancellationToken stepCancellationToken = default, CancellationToken jobCancellationToken = default);
    public abstract Task ExecuteAsync(IUnityContainer container, CancellationToken stepCancellationToken = default, CancellationToken jobCancellationToken = default);
    public abstract UserControl? GetJobStepView();
    public abstract IJobStepContext? GetJobStepDataContext();
    public abstract ImmutableResultCollection Validate(IUnityContainer container);
    public abstract Task<ImmutableResultCollection> ValidateAsync(IUnityContainer container);
}
