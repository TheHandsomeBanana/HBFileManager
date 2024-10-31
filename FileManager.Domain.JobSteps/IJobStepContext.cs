using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Domain.JobSteps;

public interface IJobStepContext {
    public string Name { get; set; }
    public bool IsAsync { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsValid { get; set; }
    public int ExecutionOrder { get; set; }
    public event Action<int, JobStep>? ExecutionOrderChanged;
    public event Func<JobStep, bool>? ValidationRequired;
    public event Func<JobStep, Task<bool>>? AsyncValidationRequired;
    public bool ValidationRunning { get; }
    public bool AsyncValidationRunning { get; }

}
