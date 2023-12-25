using HB.Code.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax;
public class CommandSyntax : ExpressionSyntax {
    public CommandSyntax(TextSpan span, SyntaxNode parent) : base(span, parent) {
    }
}
