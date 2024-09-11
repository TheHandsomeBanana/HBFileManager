using FileManager.Core.Interpreter.Exceptions;
using FileManager.Core.Interpreter.Syntax.Statements;

namespace FileManager.Core.Interpreter.Syntax.Commands;
public class CommandStatementSyntax : StatementSyntax {
    public CommandSyntax? Command { get; private set; }

    public CommandStatementSyntax() : base(SyntaxNodeKind.CommandStatement) {
    }

    public override void AddChildNode(SyntaxNode node) {
        if (this.ChildNodes.Count > 1)
            SyntaxBuilderException.ThrowMaxNodeLength(nameof(CommandStatementSyntax));

        if (node is not CommandSyntax command)
            throw new SyntaxBuilderException($"{node.GetType()} is not of type {nameof(CommandSyntax)}");

        Command = command;
        base.AddChildNode(node);
    }
}
