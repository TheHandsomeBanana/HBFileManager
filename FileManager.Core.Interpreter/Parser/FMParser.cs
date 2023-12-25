using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter;
using HB.Code.Interpreter.Lexer.Default;
using HB.Code.Interpreter.Parser;
using HB.Code.Interpreter.Parser.Default;
using HB.Code.Interpreter.Syntax;
using System.Collections.Immutable;

namespace FileManager.Core.Interpreter.Parser;
public class FMParser : IParser<SyntaxTree, SyntaxToken, DefaultSyntaxError> {
    private readonly List<DefaultSyntaxError> syntaxErrors = new List<DefaultSyntaxError>();
    private readonly DefaultTokenReader<SyntaxToken, SyntaxTokenKind> tokenReader = new DefaultTokenReader<SyntaxToken, SyntaxTokenKind>();

    public SyntaxTree Parse(ImmutableArray<SyntaxToken> tokens) {
        syntaxErrors.Clear();
        tokenReader.Init(tokens);

        SyntaxNode root = new InterpreterUnitSyntax(new TextSpan(tokens.First().Span.Start, tokens.Last().Span.End));
        root.AddChildToken(tokens.Last()); // Insert EndOfFileToken


        return new SyntaxTree(root);
    }
   

    public ImmutableArray<DefaultSyntaxError> GetSyntaxErrors() => syntaxErrors.ToImmutableArray();
}
