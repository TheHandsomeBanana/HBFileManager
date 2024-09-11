namespace FileManager.Core.JobSteps;
public class StepExecutionException : Exception {
    public StepExecutionException(string? message, bool invokeErrorDialog) : base(message) {
    }

    public StepExecutionException(string? message, Exception? innerException, bool invokeErrorDialog) : base(message, innerException) {
    }
}
