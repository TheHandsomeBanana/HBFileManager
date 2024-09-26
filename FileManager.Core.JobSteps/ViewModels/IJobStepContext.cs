using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.JobSteps.ViewModels;

public interface IJobStepContext {
    public string Name { get; }
    public bool IsAsync { get; }
    public bool IsEnabled { get; }
    public bool CanExecute { get; }
    public int ExecutionOrder { get; }
    public event Action<int, JobStep>? ExecutionOrderChanged;
}
