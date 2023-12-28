using HB.Code.Interpreter;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax;
public class CommandExpressionSyntax : ExpressionSyntax {
    public CommandModifierListSyntax? CommandModifiers { get; }
    public SyntaxToken? Command => ChildTokens.FirstOrDefault(e => e.Kind.IsCommandTokenKind());

    public CommandExpressionSyntax(SyntaxNodeKind kind) : base(kind) {
    }

   
}
