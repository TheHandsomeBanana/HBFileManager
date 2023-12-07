using HB.Code.Interpreter.Location;
using HB.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public readonly struct FMSyntaxToken : ISyntaxToken {
    public string Value { get; }
    public SyntaxTokenKind Kind { get; }
    public TextSpan Span { get; }

    public FMSyntaxToken(string value, SyntaxTokenKind kind, TextSpan span) {
        Value = value;
        Kind = kind;
        Span = span;
    }
}
