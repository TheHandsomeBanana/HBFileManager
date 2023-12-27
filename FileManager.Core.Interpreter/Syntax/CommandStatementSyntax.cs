using HB.Code.Interpreter;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax;
public class CommandStatementSyntax : ExpressionStatementSyntax {
    public CommandSyntax[] Commands { get; set; } = [];
    
    public CommandStatementSyntax(TextSpan span, SyntaxNodeKind kind) : base(span, kind) {
    }
    
}
