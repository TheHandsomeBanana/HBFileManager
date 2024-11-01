using FileManager.Domain.JobSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.JobSteps;
public interface IExecutionStateHandler {
    public RunState State { get; }
    public void WillCompleteWithWarnings();
}
