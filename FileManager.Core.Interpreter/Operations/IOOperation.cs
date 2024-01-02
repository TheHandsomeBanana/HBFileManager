using HB.Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Operations;
public abstract class IOOperation {
    public PathIndex Source { get; }
    public PathIndex? Target { get; set; }

    protected IOOperation(PathIndex source) {
        Source = source;
    }
}
