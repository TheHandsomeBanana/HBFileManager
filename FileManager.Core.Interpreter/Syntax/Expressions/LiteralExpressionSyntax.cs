using FileManager.Core.Interpreter.Exceptions;
using HB.Code.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax.Expressions;
public class LiteralExpressionSyntax : ExpressionSyntax {
    public SyntaxToken? Literal => ChildTokens.Count > 0 ? ChildTokens[0] : null;
    public LiteralExpressionSyntax(SyntaxNodeKind kind) : base(kind) {
    }

    public override void AddChildToken(SyntaxToken token) {
        if (this.ChildTokens.Count > 1)
            SyntaxBuilderException.ThrowMaxTokenLength(nameof(LiteralExpressionSyntax));

        base.AddChildToken(token);
    }
}
