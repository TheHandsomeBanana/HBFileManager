namespace FileManager.Core.Interpreter.Syntax.Statements;
public abstract class StatementSyntax : SyntaxNode {
    protected StatementSyntax(SyntaxNodeKind kind) : base(kind) {
    }
}
