using HB.Code.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax;
public class InterpreterUnitSyntax : SyntaxNode {
    public InterpreterUnitSyntax(TextSpan span, SyntaxNodeKind kind) : base(span, kind) {
    }
}
