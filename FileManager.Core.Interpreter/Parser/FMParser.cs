using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter;
using HB.Code.Interpreter.Lexer.Default;
using HB.Code.Interpreter.Parser;
using HB.Code.Interpreter.Syntax;
using System.Collections.Immutable;

namespace FileManager.Core.Interpreter.Parser;
public class FMParser : IParser<SyntaxTree, SyntaxToken, DefaultSyntaxError> {
    private readonly List<DefaultSyntaxError> syntaxErrors = new List<DefaultSyntaxError>();
    private ImmutableArray<SyntaxToken> tokens = [];
    private int currentTokenIndex = 0;

    public SyntaxTree Parse(ImmutableArray<SyntaxToken> tokens) {
        syntaxErrors.Clear();
        this.tokens = tokens;
        SyntaxNode root = new InterpreterUnitSyntax(new TextSpan(tokens.First().Span.Start, tokens.Last().Span.End));
        root.AddChildToken(tokens.Last()); // Insert EndOfFileToken
        BuildFromRoot(root);
        return new SyntaxTree(root);
    }

    private SyntaxTokenKind[] possibleNextTokens = [];

    private SyntaxNode BuildFromRoot(SyntaxNode root) {
        SyntaxToken current = tokens[currentTokenIndex];
        
        

        return null;
    }


    public ImmutableArray<DefaultSyntaxError> GetSyntaxErrors() => syntaxErrors.ToImmutableArray();
}
