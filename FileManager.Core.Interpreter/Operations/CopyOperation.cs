using FileManager.Core.Interpreter.Exceptions;
using HBLibrary.Code.Interpreter.Operation;
using HBLibrary.Services.IO;

namespace FileManager.Core.Interpreter.Operations;
public sealed class CopyOperation : IOOperation, IAsyncOperation {
    public CopyOperation(ValidPath source) : base(source) {
    }

    public Task Run() {
        if (Target is null)
            throw new OperationException("Destination not set.");

        throw new NotImplementedException();
    }
}
