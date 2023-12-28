using HB.Code.Interpreter;
using HB.Code.Interpreter.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace FileManager.Core.Interpreter.Syntax;
public readonly struct SyntaxToken : ISyntaxToken, IEquatable<SyntaxToken> {
    public string Value { get; }
    public SyntaxTokenKind Kind { get; }
    public TextSpan FullSpan { get; }
    public LineSpan LineSpan { get; }

    private readonly List<SyntaxTrivia> childTrivia = [];
    public IReadOnlyList<SyntaxTrivia> ChildTrivia => childTrivia;

    public void AddSyntaxTrivia(SyntaxTrivia syntaxTrivia) {
        childTrivia.Add(syntaxTrivia);
    }

    public SyntaxToken(string value, SyntaxTokenKind kind, TextSpan fullSpan, LineSpan lineSpan) {
        Value = value;
        Kind = kind;
        FullSpan = fullSpan;
        LineSpan = lineSpan;
    }

    public bool Equals(SyntaxToken other) {
        return this.Value == other.Value
            && this.Kind == other.Kind
            && this.FullSpan == other.FullSpan;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) 
        => obj is SyntaxToken token && Equals(token);

    public override int GetHashCode() =>  HashCode.Combine(Value, Kind, FullSpan);

    public bool IsKind(SyntaxTokenKind kind) => Kind == kind;

    public override string ToString() {
        return $"{Value} {Kind} {FullSpan} ({LineSpan})";
    }
}
