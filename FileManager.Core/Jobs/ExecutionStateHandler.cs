using FileManager.Core.JobSteps;
using FileManager.Domain;
using FileManager.Domain.JobSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Jobs;

public class ExecutionStateHandler : IExecutionStateHandler {
    public RunState State { get; set; }
    public void WillCompleteWithWarnings() {
        State = RunState.CompletedWithWarnings;
    }

    public void IsCanceled() {
        State = RunState.Canceled;
    }
}
