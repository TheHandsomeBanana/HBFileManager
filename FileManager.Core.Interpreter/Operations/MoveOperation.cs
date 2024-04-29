using FileManager.Core.Interpreter.Exceptions;
using HBLibrary.Code.Interpreter.Operation;
using HBLibrary.Services.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Operations;
public sealed class MoveOperation : IOOperation, IAsyncOperation {
    public MoveOperation(ValidPath source) : base(source) {
    }

    public async Task Run() {
        if (!Target.HasValue)
            throw new OperationException("Destination not set.");

        //await IOService.Move(Source, Target.Value);
    }
}
