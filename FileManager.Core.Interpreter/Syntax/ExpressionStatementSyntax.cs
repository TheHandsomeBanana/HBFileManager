using HB.Code.Interpreter;

namespace FileManager.Core.Interpreter.Syntax;
public class ExpressionStatementSyntax : SyntaxNode {

    public ExpressionStatementSyntax(TextSpan span, SyntaxNodeKind kind) : base(span, kind) {
    }
}
