namespace FileManager.Core.Interpreter.Syntax;
public enum SyntaxTriviaKind {
    WhiteSpace,
    Tab,
    EndOfLine,
}

public enum SyntaxTokenKind {
    EndOfFile,
    OpenBrace,
    CloseBrace,
    OpenParenthesis,
    CloseParenthesis,
    Comma,
    Pipe,
    Equals,
    Semicolon,
    NumericLiteral,
    StringLiteral,
    CopyKeyword,
    MoveKeyword,
    ReplaceKeyword,
    ArchiveKeyword,
    SourceParameter,
    TargetParameter,
    ModifiedOnlyParameter,
    TypeParameter,
}

public enum SyntaxNodeKind {
    InterpreterUnit,
    NumericLiteral,
    StringLiteral,
    Argument,
    ArgumentList,
    CommandStatement,
    CopyCommand,
    MoveCommand,
    ReplaceCommand,
    ArchiveCommand,
    CommandParameterList,
    CommandSourceParameter,
    CommandTargetParameter,
    CommandModifiedOnlyParamater,
    CommandTypeParameter,
    CommandParameterAssignment
}


