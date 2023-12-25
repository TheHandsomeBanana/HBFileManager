using HB.Code.Interpreter;

namespace FileManager.Core.Interpreter.Syntax;
public class ExpressionStatementSyntax : FMSyntaxNode {
    public ExpressionStatementSyntax(TextSpan span, FMSyntaxNode? parent = null) : base(span, parent) {
    }
}
