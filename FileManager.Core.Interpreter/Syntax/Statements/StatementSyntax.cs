using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax.Statements;
public abstract class StatementSyntax : SyntaxNode {
    protected StatementSyntax(SyntaxNodeKind kind) : base(kind) {
    }
}
