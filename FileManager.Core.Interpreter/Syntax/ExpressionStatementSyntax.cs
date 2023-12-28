using HB.Code.Interpreter;

namespace FileManager.Core.Interpreter.Syntax;
public sealed class ExpressionStatementSyntax : SyntaxNode {
    public ExpressionSyntax? Expression { get; set; }
    public ExpressionStatementSyntax(SyntaxNodeKind kind) : base(kind) {
    }

}
