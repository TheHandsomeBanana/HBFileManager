namespace FileManager.Core.Interpreter.Syntax.Expressions;
public abstract class ExpressionSyntax : SyntaxNode {
    protected ExpressionSyntax(SyntaxNodeKind kind) : base(kind) {
    }
}
