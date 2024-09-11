using FileManager.Core.Interpreter.Exceptions;

namespace FileManager.Core.Interpreter.Syntax.Statements;
public class BlockSyntax : SyntaxNode {
    private readonly List<StatementSyntax> statements = [];
    public IReadOnlyList<StatementSyntax> Statements => statements;
    public BlockSyntax(SyntaxNodeKind kind) : base(kind) {
    }

    public override void AddChildNode(SyntaxNode node) {
        if (node is not StatementSyntax statement)
            throw SyntaxBuilderException.NodeNotTypeOf(node, nameof(StatementSyntax));

        statements.Add(statement);
        base.AddChildNode(node);
    }
}
