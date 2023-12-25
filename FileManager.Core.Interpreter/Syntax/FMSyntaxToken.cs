using HB.Code.Interpreter;
using HB.Code.Interpreter.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace FileManager.Core.Interpreter.Syntax;
public readonly struct FMSyntaxToken : ISyntaxToken, IEquatable<FMSyntaxToken> {
    public string Value { get; }
    public SyntaxTokenKind Kind { get; }
    public TextSpan Span { get; }

    public FMSyntaxToken(string value, SyntaxTokenKind kind, TextSpan span) {
        Value = value;
        Kind = kind;
        Span = span;
    }

    public bool Equals(FMSyntaxToken other) {
        return this.Value == other.Value
            && this.Kind == other.Kind
            && this.Span == other.Span;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) 
        => obj is FMSyntaxToken token && Equals(token);

    public override int GetHashCode() =>  HashCode.Combine(Value, Kind, Span);
}
