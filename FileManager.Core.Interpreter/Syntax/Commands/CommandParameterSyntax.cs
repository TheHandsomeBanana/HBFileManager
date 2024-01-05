using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileManager.Core.Interpreter.Exceptions;
using FileManager.Core.Interpreter.Syntax.Arguments;
using FileManager.Core.Interpreter.Syntax.Expressions;

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
