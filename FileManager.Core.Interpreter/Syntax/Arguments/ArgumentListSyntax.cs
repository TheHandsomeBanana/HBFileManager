using FileManager.Core.Interpreter.Exceptions;
using FileManager.Core.Interpreter.Syntax.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
