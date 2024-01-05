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
    SourceParameter,
    TargetParameter,
    ModifiedOnlyParameter,
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
    CommandParameterList,
    CommandSourceParameter,
    CommandTargetParameter,
    CommandModifiedOnlyParamater,
    CommandParameterAssignment
}


