using HB.Code.Interpreter;

namespace FileManager.Core.Interpreter.Syntax;
public class ExpressionStatementSyntax : SyntaxNode {
    public ExpressionSyntax[] Expressions { get; } = [];
    public ExpressionStatementSyntax(TextSpan span, SyntaxNode parent) : base(span, parent) {
    }
}
