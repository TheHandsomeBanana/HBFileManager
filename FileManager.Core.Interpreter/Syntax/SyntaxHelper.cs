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
        || kind == SyntaxTokenKind.ToKeyword;

    public static bool IsCommandExecutorTokenKind(this SyntaxTokenKind kind)
        => kind == SyntaxTokenKind.ToKeyword;

    public static bool IsCommandModifierTokenKind(this SyntaxTokenKind kind) 
        => kind == SyntaxTokenKind.FileModifierKeyword
        || kind == SyntaxTokenKind.DirectoryModifierKeyword
        || kind == SyntaxTokenKind.ModifiedOnlyModifierKeyword;
}
