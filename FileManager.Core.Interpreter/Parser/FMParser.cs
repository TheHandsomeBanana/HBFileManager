using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter.Lexer.Default;
using HB.Code.Interpreter.Parser;
using HB.Code.Interpreter.Syntax;
using System.Collections.Immutable;

namespace FileManager.Core.Interpreter.Parser;
public class FMParser : IParser<FMSyntaxTree, FMSyntaxToken, DefaultSyntaxError> {
    private ImmutableArray<FMSyntaxToken> tokens = [];
    private int currentTokenIndex = 0;

    public FMSyntaxTree Parse(ImmutableArray<FMSyntaxToken> tokens) {
        this.tokens = tokens;

        FMSyntaxNode root = BuildSyntaxTree();
        return new FMSyntaxTree(root);
    }

    private SyntaxTokenKind[] possibleNextTokens = [];

    private FMSyntaxNode BuildSyntaxTree() {
        switch (tokens[currentTokenIndex].Kind) {
            case SyntaxTokenKind.WhiteSpace:
                break;
            case SyntaxTokenKind.EndOfLine:
                break;
            case SyntaxTokenKind.EndOfFile:
                break;
            case SyntaxTokenKind.Semicolon:
                break;
            case SyntaxTokenKind.NumericLiteral:
                break;
            case SyntaxTokenKind.StringLiteral:
                break;
            case SyntaxTokenKind.ToKeyword:
                break;
            case SyntaxTokenKind.FromKeyword:
                break;
            case SyntaxTokenKind.FileKeyword:
                break;
            case SyntaxTokenKind.DirectoryKeyword:
                break;
            case SyntaxTokenKind.CopyKeyword:
                break;
            case SyntaxTokenKind.MoveKeyword:
                break;
            default:
                break;
        }

        return null;
    }


    private readonly List<DefaultSyntaxError> errors = new List<DefaultSyntaxError>();
    public ImmutableArray<DefaultSyntaxError> GetSyntaxErrors() {
        throw new NotImplementedException();
    }
}
