using FileManager.Core.Interpreter.Exceptions;
using HB.Code.Interpreter.Operation;
using HB.Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Operations;
public sealed class CopyOperation : IOOperation, IAsyncOperation {
    public CopyOperation(PathIndex source) : base(source) {
    }

    public async Task Run() {
        if (!Target.HasValue)
            throw new OperationException("Destination not set.");

        await IOService.Copy(Source, Target.Value);
    }
}
