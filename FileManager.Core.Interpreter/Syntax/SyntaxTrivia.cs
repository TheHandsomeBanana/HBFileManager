
using HBLibrary.Interface.Interpreter;
using HBLibrary.Interface.Interpreter.Syntax;

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
