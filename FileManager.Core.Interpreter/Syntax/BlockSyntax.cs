using HB.Code.Interpreter;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax;
public class BlockSyntax : SyntaxNode {
    public ExpressionStatementSyntax[] Statements => ChildNodes.OfType<ExpressionStatementSyntax>().ToArray();
    public BlockSyntax(SyntaxNodeKind kind) : base(kind) {
    }


}
