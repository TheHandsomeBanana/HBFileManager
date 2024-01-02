using FileManager.Core.Interpreter.Exceptions;
using HB.Code.Interpreter.Operation;
using HB.Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Operations;
public sealed class MoveOperation : IOOperation, IAsyncOperation {
    public MoveOperation(PathIndex source) : base(source) {
    }

    public async Task Run() {
        if (!Target.HasValue)
            throw new OperationException("Destination not set.");

        await IOService.Move(Source, Target.Value);
    }
}
