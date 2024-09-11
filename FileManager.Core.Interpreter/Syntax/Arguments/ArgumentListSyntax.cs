using FileManager.Core.Interpreter.Exceptions;

namespace FileManager.Core.Interpreter.Syntax.Arguments;
public class ArgumentListSyntax : SyntaxNode {
    private readonly List<ArgumentSyntax> arguments = [];
    public IReadOnlyList<ArgumentSyntax> Arguments => arguments;
    public ArgumentListSyntax(SyntaxNodeKind kind) : base(kind) {
    }

    public override void AddChildNode(SyntaxNode node) {
        if (node is not ArgumentSyntax argument)
            throw SyntaxBuilderException.NodeNotTypeOf(node, nameof(ArgumentSyntax));

        this.arguments.Add(argument);
        base.AddChildNode(node);
    }


}
