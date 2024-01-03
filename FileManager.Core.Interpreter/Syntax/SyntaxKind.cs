namespace FileManager.Core.Interpreter.Syntax;
public enum SyntaxTriviaKind {
    WhiteSpace,
    Tab,
    EndOfLine,
}

public enum SyntaxTokenKind {
    BlockStart,
    BlockEnd,
    EndOfFile,
    Semicolon,
    NumericLiteral,
    StringLiteral,
    ToKeyword,
    CopyKeyword,
    MoveKeyword,
    ReplaceKeyword,
    FileModifierKeyword,
    DirectoryModifierKeyword,
    ModifiedOnlyModifierKeyword,
}

public enum SyntaxNodeKind {
    InterpreterUnit,
    NumericLiteral,
    StringLiteral,
    CommandBlock,
    ExpressionStatement,
    CopyCommandExpression,
    MoveCommandExpression,
    ReplaceCommandExpression,
    ToCommandExpression,
    CommandModifierList
}


