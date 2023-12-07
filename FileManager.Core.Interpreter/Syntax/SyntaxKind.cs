namespace FileManager.Core.Interpreter.Syntax;
public enum SyntaxTokenKind {
    WhiteSpace,
    EndOfLine,
    EndOfFile,
    Semicolon,
    NumericLiteral,
    StringLiteral,
    ToKeyword,
    FromKeyword,
    FileKeyword,
    DirectoryKeyword,
    CopyKeyword,
    MoveKeyword
}

public enum SyntaxNodeKind {
    NumericLiteral,
    StringLiteral,
    CommandStatement,

}


