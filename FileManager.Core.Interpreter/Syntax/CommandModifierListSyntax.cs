using HB.Code.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax;
public sealed class CommandModifierListSyntax : SyntaxNode {
    public SyntaxToken[] CommandModifiers => ChildTokens.ToArray();
    public CommandModifierListSyntax(SyntaxNodeKind kind) : base(kind) {
    }
}
