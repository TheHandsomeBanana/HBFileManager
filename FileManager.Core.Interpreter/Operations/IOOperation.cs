
using HBLibrary.Interface.IO;

namespace FileManager.Core.Interpreter.Operations;
public abstract class IOOperation {
    public ValidPath Source { get; }
    public ValidPath? Target { get; set; }

    protected IOOperation(ValidPath source) {
        Source = source;
    }
}
