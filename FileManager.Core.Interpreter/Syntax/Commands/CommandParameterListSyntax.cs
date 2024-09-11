using FileManager.Core.Interpreter.Exceptions;

namespace FileManager.Core.Interpreter.Syntax.Commands;
public sealed class CommandParameterListSyntax : SyntaxNode {
    private readonly List<CommandParameterSyntax> parameters = [];
    public IReadOnlyList<CommandParameterSyntax> Parameters => parameters;
    public CommandParameterListSyntax(SyntaxNodeKind kind) : base(kind) {
    }

    public override void AddChildNode(SyntaxNode node) {
        if (node is not CommandParameterSyntax commandParameter)
            throw SyntaxBuilderException.NodeNotTypeOf(node, nameof(CommandParameterSyntax));

        parameters.Add(commandParameter);
        base.AddChildNode(node);
    }
}
