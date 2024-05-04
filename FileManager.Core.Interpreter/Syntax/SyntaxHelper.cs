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
        || kind == SyntaxTokenKind.ReplaceKeyword
        || kind == SyntaxTokenKind.ArchiveKeyword;

    public static bool IsCommandParameterTokenKind(this SyntaxTokenKind kind)
        => kind == SyntaxTokenKind.SourceParameter
        || kind == SyntaxTokenKind.TargetParameter
        || kind == SyntaxTokenKind.ModifiedOnlyParameter
        || kind == SyntaxTokenKind.TypeParameter;

    public static bool IsArgumentTokenKind(this SyntaxTokenKind kind)
        => kind == SyntaxTokenKind.NumericLiteral
        || kind == SyntaxTokenKind.StringLiteral;

    public static SyntaxNodeKind GetNodeKind(this SyntaxTokenKind kind) {
        return kind switch {
            SyntaxTokenKind.MoveKeyword => SyntaxNodeKind.MoveCommand,
            SyntaxTokenKind.ReplaceKeyword => SyntaxNodeKind.ReplaceCommand,
            SyntaxTokenKind.CopyKeyword => SyntaxNodeKind.CopyCommand,
            SyntaxTokenKind.ArchiveKeyword => SyntaxNodeKind.ArchiveCommand,
            SyntaxTokenKind.TargetParameter => SyntaxNodeKind.CommandTargetParameter,
            SyntaxTokenKind.SourceParameter => SyntaxNodeKind.CommandSourceParameter,
            SyntaxTokenKind.ModifiedOnlyParameter => SyntaxNodeKind.CommandModifiedOnlyParamater,
            SyntaxTokenKind.TypeParameter => SyntaxNodeKind.CommandTypeParameter,
            SyntaxTokenKind.StringLiteral => SyntaxNodeKind.StringLiteral,
            SyntaxTokenKind.NumericLiteral => SyntaxNodeKind.NumericLiteral,
            _ => throw new NotSupportedException($"{kind}")
        };
    }

    public static SyntaxTokenKind GetTokenKind(this SyntaxNodeKind kind) {
        return kind switch {
            SyntaxNodeKind.MoveCommand => SyntaxTokenKind.MoveKeyword,
            SyntaxNodeKind.ReplaceCommand => SyntaxTokenKind.ReplaceKeyword,
            SyntaxNodeKind.CopyCommand => SyntaxTokenKind.CopyKeyword,
            SyntaxNodeKind.ArchiveCommand => SyntaxTokenKind.ArchiveKeyword,
            SyntaxNodeKind.CommandTargetParameter => SyntaxTokenKind.TargetParameter,
            SyntaxNodeKind.CommandSourceParameter => SyntaxTokenKind.SourceParameter,
            SyntaxNodeKind.CommandModifiedOnlyParamater => SyntaxTokenKind.ModifiedOnlyParameter,
            SyntaxNodeKind.CommandTypeParameter => SyntaxTokenKind.TypeParameter,
            SyntaxNodeKind.StringLiteral => SyntaxTokenKind.StringLiteral,
            SyntaxNodeKind.NumericLiteral => SyntaxTokenKind.NumericLiteral,
            _ => throw new NotSupportedException($"{kind}")
        };
    }

    public static SyntaxNodeKind GetNodeKind(this SyntaxToken token)
        => GetNodeKind(token.Kind);

    public static SyntaxTokenKind GetTokenKind(this SyntaxNode node)
        => GetTokenKind(node.Kind);
}
