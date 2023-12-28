using HB.Code.Interpreter;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax;
public sealed class CommandBlockSyntax : BlockSyntax {
    public CommandBlockSyntax(SyntaxNodeKind kind) : base(kind) {
    }
}
