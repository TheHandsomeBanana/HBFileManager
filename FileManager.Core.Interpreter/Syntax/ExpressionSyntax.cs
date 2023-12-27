using HB.Code.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax;
public class ExpressionSyntax : SyntaxNode {
    public ExpressionSyntax(TextSpan span, SyntaxNodeKind kind) : base(span, kind) {
    }
}
