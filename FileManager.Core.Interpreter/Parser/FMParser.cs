using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter;
using HB.Code.Interpreter.Lexer.Default;
using HB.Code.Interpreter.Parser;
using HB.Code.Interpreter.Parser.Default;
using HB.Code.Interpreter.Syntax;
using System.Collections.Immutable;

namespace FileManager.Core.Interpreter.Parser;
public class FMParser : IParser<SyntaxTree, SyntaxToken, DefaultSyntaxError> {
    private readonly List<DefaultSyntaxError> syntaxErrors = [];
    private readonly DefaultTokenReader<SyntaxToken, SyntaxTokenKind> tokenReader = new();

    public SyntaxTree Parse(ImmutableArray<SyntaxToken> tokens) {
        syntaxErrors.Clear();
        tokenReader.Init(tokens);


        SyntaxNode root = new InterpreterUnitSyntax(new TextSpan(tokens.First().Span.Start, tokens.Last().Span.End), SyntaxNodeKind.InterpreterUnit);
        root.AddChildToken(tokens.Last()); // Insert EndOfFileToken
        root.AddChildNode(BuildSyntaxTree(tokenReader.CurrentToken));
            

        return new SyntaxTree(root);
    }

    public ImmutableArray<DefaultSyntaxError> GetSyntaxErrors() => syntaxErrors.ToImmutableArray();

    #region Parsing
    private SyntaxNode BuildSyntaxTree(SyntaxToken current) {

        switch (current.Kind) {
            case SyntaxTokenKind.WhiteSpace:
            case SyntaxTokenKind.EndOfLine:
                break;
            case SyntaxTokenKind.EndOfFile:
                break;
            case SyntaxTokenKind.Semicolon:
                break;
            case SyntaxTokenKind.NumericLiteral:
                LiteralExpressionSyntax numericLiteral = new LiteralExpressionSyntax(current.Span, SyntaxNodeKind.NumericLiteral);
                numericLiteral.AddChildToken(current);
                return numericLiteral;
            case SyntaxTokenKind.StringLiteral:
                LiteralExpressionSyntax stringLiteral = new LiteralExpressionSyntax(current.Span, SyntaxNodeKind.StringLiteral);
                stringLiteral.AddChildToken(current);
                return stringLiteral;
            case SyntaxTokenKind.ToKeyword:
                break;
            case SyntaxTokenKind.FileModifierKeyword:
                break;
            case SyntaxTokenKind.DirectoryModifierKeyword:
                break;
            case SyntaxTokenKind.CopyKeyword:
                break;
            case SyntaxTokenKind.MoveKeyword:
                break;
            case SyntaxTokenKind.ReplaceKeyword:
                break;
            default:
                break;
        }
    }

    private void Build(SyntaxToken current) {
        switch (current.Kind) {
            // Skip
            case SyntaxTokenKind.WhiteSpace:
            case SyntaxTokenKind.EndOfLine:
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
            case SyntaxTokenKind.FileModifierKeyword:
                break;
            case SyntaxTokenKind.DirectoryModifierKeyword:
                break;
            case SyntaxTokenKind.CopyKeyword:
                break;
            case SyntaxTokenKind.MoveKeyword:
                break;
            case SyntaxTokenKind.ReplaceKeyword:
                break;
            default:
                break;
        }
    }
    #endregion
}
