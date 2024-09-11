using FileManager.Core.Interpreter.Exceptions;

namespace FileManager.Core.Interpreter.Syntax.Commands;
public class CommandParameterSyntax : SyntaxNode {
    public CommandParameterAssignmentSyntax? ParameterAssignment { get; private set; }
    public CommandParameterSyntax(SyntaxNodeKind kind) : base(kind) {
    }

    public override void AddChildNode(SyntaxNode node) {
        if (this.ChildNodes.Count > 1)
            SyntaxBuilderException.ThrowMaxNodeLength(nameof(CommandParameterSyntax));

        if (node is not CommandParameterAssignmentSyntax commandParameter)
            throw new SyntaxBuilderException($"{node.GetType()} is not of type {nameof(CommandParameterAssignmentSyntax)}");

        ParameterAssignment = commandParameter;
        base.AddChildNode(node);
    }
}
