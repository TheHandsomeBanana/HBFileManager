using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax;
public class CommandExecutorExpressionSyntax : CommandExpressionSyntax {
    public CommandExecutorExpressionSyntax(SyntaxNodeKind kind) : base(kind) {
    }
}
