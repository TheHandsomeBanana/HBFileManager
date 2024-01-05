using FileManager.Core.Interpreter.Exceptions;
using FileManager.Core.Interpreter.Syntax.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax.Commands;
public class CommandSyntax : SyntaxNode {
    public CommandParameterListSyntax? ParameterList { get; private set; }
    public SyntaxToken? Command => ChildTokens.Count > 0 ? ChildTokens[0] : null;
    public CommandSyntax(SyntaxNodeKind kind) : base(kind) {
    }

    public override void AddChildNode(SyntaxNode node) {
        if (this.ChildNodes.Count > 1)
            SyntaxBuilderException.ThrowMaxNodeLength(nameof(CommandSyntax));

        if (node is not CommandParameterListSyntax parameterList)
            throw SyntaxBuilderException.NodeNotTypeOf(node, nameof(CommandParameterListSyntax));

        this.ParameterList = parameterList;
        base.AddChildNode(node);
    }
}
