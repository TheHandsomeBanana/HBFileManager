using HBLibrary.Code.Interpreter;
using HBLibrary.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public readonly struct SyntaxTrivia : ISyntaxTrivia {
    public bool Leading { get; }
    public TextSpan FullSpan { get; }
    public SyntaxTriviaKind Kind { get; }
    public SyntaxTrivia(bool leading, SyntaxTriviaKind kind, TextSpan fullSpan) {
        Leading = leading;
        Kind = kind;
        FullSpan = fullSpan;
    }
}
