using FileManager.Core.Interpreter.Exceptions;

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
