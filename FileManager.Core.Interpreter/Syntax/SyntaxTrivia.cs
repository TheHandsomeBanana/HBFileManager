using HBLibrary.Code.Interpreter;
using HBLibrary.Code.Interpreter.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
