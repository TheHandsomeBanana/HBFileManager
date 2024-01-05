using FileManager.Core.Interpreter.Exceptions;
using FileManager.Core.Interpreter.Syntax.Commands;
using FileManager.Core.Interpreter.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax.Arguments;
public class ArgumentSyntax : SyntaxNode {
    public ExpressionSyntax? Argument { get; private set; }
    public ArgumentSyntax(SyntaxNodeKind kind) : base(kind) {
    }

    public override void AddChildNode(SyntaxNode node) {
        if (this.ChildNodes.Count > 1)
            SyntaxBuilderException.ThrowMaxNodeLength(nameof(ArgumentSyntax));

        if (node is not ExpressionSyntax expression)
            throw SyntaxBuilderException.NodeNotTypeOf(node, nameof(ExpressionSyntax));

        this.Argument = expression;
        base.AddChildNode(node);
    }
}
