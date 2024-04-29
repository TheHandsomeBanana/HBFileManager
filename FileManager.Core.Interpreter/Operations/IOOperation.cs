using HBLibrary.Services.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Operations;
public abstract class IOOperation {
    public ValidPath Source { get; }
    public ValidPath? Target { get; set; }

    protected IOOperation(ValidPath source) {
        Source = source;
    }
}
