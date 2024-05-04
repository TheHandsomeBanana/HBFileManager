using FileManager.Core.Interpreter.Syntax;
using HBLibrary.Services.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Evaluator;
public static class FMSemanticRuleset {
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
