using HB.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Exceptions;
public class OperationException : InternalException {
    public OperationException(string? message) : base("Operation failed: " + message) {
    }
}
