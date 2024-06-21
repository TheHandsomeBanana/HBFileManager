using FileManager.Core.Interpreter.Syntax;
using HBLibrary.Services.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter;
public static class FMSemanticRuleset {
    public readonly static Dictionary<SyntaxTokenKind, SyntaxTokenKind[]> TTFollowups = new() {
        // Basic tokens
        [SyntaxTokenKind.EndOfFile] = [],

        [SyntaxTokenKind.OpenParenthesis] = [SyntaxTokenKind.StringLiteral,
            SyntaxTokenKind.NumericLiteral],

        [SyntaxTokenKind.CloseParenthesis] = [SyntaxTokenKind.Semicolon,
            SyntaxTokenKind.TargetParameter,
            SyntaxTokenKind.SourceParameter,
            SyntaxTokenKind.TypeParameter,
            SyntaxTokenKind.ModifiedOnlyParameter],

        [SyntaxTokenKind.Comma] = [SyntaxTokenKind.StringLiteral, 
            SyntaxTokenKind.NumericLiteral],

        [SyntaxTokenKind.Semicolon] = [SyntaxTokenKind.ArchiveKeyword,
            SyntaxTokenKind.CopyKeyword,
            SyntaxTokenKind.MoveKeyword,
            SyntaxTokenKind.ReplaceKeyword,
            SyntaxTokenKind.EndOfFile],

        [SyntaxTokenKind.Equals] = [SyntaxTokenKind.OpenParenthesis,
            SyntaxTokenKind.StringLiteral,
            SyntaxTokenKind.NumericLiteral],

        // Literals
        [SyntaxTokenKind.NumericLiteral] = [SyntaxTokenKind.Comma,
            SyntaxTokenKind.Semicolon,
            SyntaxTokenKind.CloseParenthesis],

        [SyntaxTokenKind.StringLiteral] = [SyntaxTokenKind.Comma,
            SyntaxTokenKind.Semicolon,
            SyntaxTokenKind.CloseParenthesis],

        // Commands
        [SyntaxTokenKind.CopyKeyword] = [SyntaxTokenKind.ModifiedOnlyParameter,
            SyntaxTokenKind.SourceParameter,
            SyntaxTokenKind.TargetParameter],

        [SyntaxTokenKind.MoveKeyword] = [SyntaxTokenKind.ModifiedOnlyParameter,
            SyntaxTokenKind.SourceParameter,
            SyntaxTokenKind.TargetParameter],

        [SyntaxTokenKind.ReplaceKeyword] = [SyntaxTokenKind.ModifiedOnlyParameter,
            SyntaxTokenKind.SourceParameter,
            SyntaxTokenKind.TargetParameter],

        [SyntaxTokenKind.ArchiveKeyword] = [SyntaxTokenKind.TypeParameter,
            SyntaxTokenKind.SourceParameter,
            SyntaxTokenKind.TargetParameter],


        // Command parameters
        [SyntaxTokenKind.TargetParameter] = [SyntaxTokenKind.Equals],

        [SyntaxTokenKind.SourceParameter] = [SyntaxTokenKind.Equals],

        [SyntaxTokenKind.TypeParameter] = [SyntaxTokenKind.Equals],

        [SyntaxTokenKind.ModifiedOnlyParameter] = [SyntaxTokenKind.Semicolon,
            SyntaxTokenKind.SourceParameter,
            SyntaxTokenKind.TypeParameter,
            SyntaxTokenKind.TargetParameter]
    };

    public readonly static Dictionary<SyntaxNodeKind, SyntaxTokenKind[]> NTFollowups = new() {
        // Commands
        [SyntaxNodeKind.ArchiveCommand] = [SyntaxTokenKind.Semicolon],

        [SyntaxNodeKind.CopyCommand] = [SyntaxTokenKind.Semicolon],

        [SyntaxNodeKind.MoveCommand] = [SyntaxTokenKind.Semicolon],

        [SyntaxNodeKind.ReplaceCommand] = [SyntaxTokenKind.Semicolon],

        // Command parameters
        [SyntaxNodeKind.CommandParameterList] = [],
        [SyntaxNodeKind.CommandParameterAssignment] = [],

        // Others
        [SyntaxNodeKind.InterpreterUnit] = [],

        [SyntaxNodeKind.Argument] = [SyntaxTokenKind.Comma],
    };

    public readonly static Dictionary<SyntaxNodeKind, SyntaxNodeKind[]> NNFollowups = new() {
        [SyntaxNodeKind.CommandStatement] = [SyntaxNodeKind.CommandStatement]
    };

    public static bool CheckTokenToTokenFollowup(SyntaxTokenKind tokenKind, SyntaxTokenKind followTokenKind)
        => TTFollowups.ContainsKey(tokenKind) && TTFollowups[tokenKind].Contains(followTokenKind);

    public static bool CheckTokenToNodeFollowup(SyntaxNodeKind nodeKind, SyntaxTokenKind followTokenKind)
        => NTFollowups.ContainsKey(nodeKind) && NTFollowups[nodeKind].Contains(followTokenKind);

    public static bool CheckNodeToNodeFollowup(SyntaxNodeKind nodeKind, SyntaxNodeKind followNodeKind)
        => NNFollowups.ContainsKey(nodeKind) && NNFollowups[nodeKind].Contains(followNodeKind);

    public readonly static Dictionary<SyntaxNodeKind, SyntaxTokenKind[]> ValidCommandParameters = new() {
        [SyntaxNodeKind.ArchiveCommand] = [SyntaxTokenKind.SourceParameter,
            SyntaxTokenKind.TargetParameter,
            SyntaxTokenKind.TypeParameter],

        [SyntaxNodeKind.CopyCommand] = [SyntaxTokenKind.SourceParameter,
            SyntaxTokenKind.TargetParameter,
            SyntaxTokenKind.ModifiedOnlyParameter],

        [SyntaxNodeKind.MoveCommand] = [SyntaxTokenKind.SourceParameter,
            SyntaxTokenKind.TargetParameter,
            SyntaxTokenKind.ModifiedOnlyParameter],

        [SyntaxNodeKind.ReplaceCommand] = [SyntaxTokenKind.SourceParameter,
            SyntaxTokenKind.TargetParameter,
            SyntaxTokenKind.ModifiedOnlyParameter],
    };

    public static bool CheckValidCommandParameter(SyntaxNodeKind commandKind, SyntaxTokenKind parameterKind)
        => ValidCommandParameters.ContainsKey(commandKind) && ValidCommandParameters[commandKind].Contains(parameterKind);

    public static bool CheckValidCommandParameter(SyntaxNodeKind commandKind, SyntaxNodeKind parameterKind) {
        SyntaxTokenKind parameterTokenKind = parameterKind.GetTokenKind();
        return CheckValidCommandParameter(commandKind, parameterTokenKind);
    }

    public static bool CheckAssignableParameter(SyntaxNodeKind commandKind, SyntaxNodeKind parameterKind, string value) {
        return commandKind switch {
            SyntaxNodeKind.ArchiveCommand => CheckArchiveCommandAssignableParameter(parameterKind, value),
            SyntaxNodeKind.CopyCommand => CheckCopyCommandAssignableParameter(parameterKind, value),
            SyntaxNodeKind.MoveCommand => CheckMoveCommandAssignableParameter(parameterKind, value),
            SyntaxNodeKind.ReplaceCommand => CheckReplaceCommandAssignableParameter(parameterKind, value),
            _ => false,
        };
    }

    private static bool CheckCopyCommandAssignableParameter(SyntaxNodeKind parameterKind, string value) {
        return parameterKind switch {
            SyntaxNodeKind.CommandSourceParameter => Directory.Exists(value) || File.Exists(value),
            SyntaxNodeKind.CommandTargetParameter => PathValidator.ValidatePath(value),
            _ => false,
        };
    }

    private static bool CheckMoveCommandAssignableParameter(SyntaxNodeKind parameterKind, string value)
        => CheckCopyCommandAssignableParameter(parameterKind, value);

    private static bool CheckReplaceCommandAssignableParameter(SyntaxNodeKind parameterKind, string value)
        => CheckCopyCommandAssignableParameter(parameterKind, value);

    private static bool CheckArchiveCommandAssignableParameter(SyntaxNodeKind parameterKind, string value) {
        return parameterKind switch {
            SyntaxNodeKind.CommandSourceParameter => Directory.Exists(value) || File.Exists(value),
            SyntaxNodeKind.CommandTargetParameter => PathValidator.ValidatePath(value),
            SyntaxNodeKind.CommandTypeParameter =>
                                value.Equals("zip", StringComparison.CurrentCultureIgnoreCase) ||
                                value.Equals("rar", StringComparison.CurrentCultureIgnoreCase),
            _ => false,
        };
    }
}
