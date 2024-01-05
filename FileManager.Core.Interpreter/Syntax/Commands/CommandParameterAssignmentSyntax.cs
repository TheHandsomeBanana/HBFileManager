using FileManager.Core.Interpreter.Exceptions;
using FileManager.Core.Interpreter.Syntax.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax.Commands;
public class CommandParameterAssignmentSyntax : SyntaxNode {
    public ArgumentListSyntax? ArgumentList { get; private set; }

    public CommandParameterAssignmentSyntax(SyntaxNodeKind kind) : base(kind) {
    }

    public override void AddChildNode(SyntaxNode node) {
        if (this.ChildNodes.Count > 1)
            SyntaxBuilderException.ThrowMaxNodeLength(nameof(CommandParameterAssignmentSyntax));

        if (node is not ArgumentListSyntax argumentList)
            throw SyntaxBuilderException.NodeNotTypeOf(node, nameof(ArgumentListSyntax));

        this.ArgumentList = argumentList;
        base.AddChildNode(node);
    }
}
