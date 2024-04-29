using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax.Expressions;
public abstract class ExpressionSyntax : SyntaxNode {
    protected ExpressionSyntax(SyntaxNodeKind kind) : base(kind) {
    }
}
