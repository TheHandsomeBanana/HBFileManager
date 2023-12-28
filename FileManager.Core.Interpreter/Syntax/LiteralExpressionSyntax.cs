using HB.Code.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax;
public class LiteralExpressionSyntax : SyntaxNode {
    public SyntaxToken Literal { get; }
    public LiteralExpressionSyntax(SyntaxNodeKind kind) : base(kind) {
    }
}
