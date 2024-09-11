namespace FileManager.Core.Interpreter.Exceptions;
public class OperationException : Exception {
    public OperationException(string? message) : base("Operation failed: " + message) {
    }
}
