using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.JobSteps;
public class StepExecutionException : Exception {
    public StepExecutionException(string? message, bool invokeErrorDialog) : base(message) {
    }

    public StepExecutionException(string? message, Exception? innerException, bool invokeErrorDialog) : base(message, innerException) {
    }
}
