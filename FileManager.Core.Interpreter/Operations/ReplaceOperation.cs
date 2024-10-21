using FileManager.Core.Interpreter.Exceptions;
using HBLibrary.Interface.Interpreter.Operation;
using HBLibrary.Interface.IO;

namespace FileManager.Core.Interpreter.Operations;
public sealed class ReplaceOperation : IOOperation, IAsyncOperation {
    public ReplaceOperation(ValidPath source) : base(source) {
    }

    public Task Run() {
        if (Target is null)
            throw new OperationException("Destination not set.");

        throw new NotImplementedException();
    }
}
