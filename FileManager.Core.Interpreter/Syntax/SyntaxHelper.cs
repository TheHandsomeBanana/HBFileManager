using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Syntax;
public static class SyntaxHelper {
    public static bool IsCommandTokenKind(this SyntaxTokenKind kind)
        => kind == SyntaxTokenKind.CopyKeyword
        || kind == SyntaxTokenKind.MoveKeyword
        || kind == SyntaxTokenKind.ReplaceKeyword;

    public static bool IsCommandParameterTokenKind(this SyntaxTokenKind kind) 
        => kind == SyntaxTokenKind.SourceParameter
        || kind == SyntaxTokenKind.TargetParameter
        || kind == SyntaxTokenKind.ModifiedOnlyParameter;

    public static bool IsArgumentTokenKind(this SyntaxTokenKind kind)
        => kind == SyntaxTokenKind.NumericLiteral
        || kind == SyntaxTokenKind.StringLiteral;

    public static SyntaxNodeKind GetNodeKind(this SyntaxTokenKind kind) {
        return kind switch {
            SyntaxTokenKind.MoveKeyword => SyntaxNodeKind.MoveCommand,
            SyntaxTokenKind.ReplaceKeyword => SyntaxNodeKind.ReplaceCommand,
            SyntaxTokenKind.CopyKeyword => SyntaxNodeKind.CopyCommand,
            SyntaxTokenKind.TargetParameter => SyntaxNodeKind.CommandTargetParameter,
            SyntaxTokenKind.SourceParameter => SyntaxNodeKind.CommandSourceParameter,
            SyntaxTokenKind.ModifiedOnlyParameter => SyntaxNodeKind.CommandModifiedOnlyParamater,
            SyntaxTokenKind.StringLiteral => SyntaxNodeKind.StringLiteral,
            SyntaxTokenKind.NumericLiteral => SyntaxNodeKind.NumericLiteral,
            _ => throw new NotSupportedException($"{kind}")
        };
    }

    public static SyntaxNodeKind GetNodeKind(this SyntaxToken token)
        => GetNodeKind(token.Kind);

}
