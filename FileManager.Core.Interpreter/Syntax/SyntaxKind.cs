namespace FileManager.Core.Interpreter.Syntax;
public enum SyntaxTokenKind {
    WhiteSpace,
    EndOfLine,
    EndOfFile,
    Semicolon,
    NumericLiteral,
    StringLiteral,
    ToKeyword,
    FileModifierKeyword,
    DirectoryModifierKeyword,
    CopyKeyword,
    MoveKeyword,
    ReplaceKeyword
}

public enum SyntaxNodeKind {
    NumericLiteral,
    StringLiteral,
    CommandStatement,

}


