using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter;
using HB.Code.Interpreter.Exceptions;
using HB.Code.Interpreter.Lexer;
using HB.Code.Interpreter.Lexer.Default;
using HB.Code.Interpreter.Syntax;
using System.Collections.Immutable;
using System.Text;
using Unity;

namespace FileManager.Core.Interpreter.Lexer;
public class FMLexer : ILexer<SyntaxToken, DefaultSyntaxError> {
    private readonly List<DefaultSyntaxError> syntaxErrors = new List<DefaultSyntaxError>();
    private readonly DefaultPositionHandler PositionHandler = new DefaultPositionHandler();
    public ImmutableArray<SyntaxToken> Lex(string input) {
        syntaxErrors.Clear();
        PositionHandler.Init(input);

        List<SyntaxToken> tokens = [];

        PositionHandler.MoveNext(1);
        while (PositionHandler.CurrentPosition.Index < PositionHandler.Content.Length) {
            SyntaxToken? token = GetNextToken();

            if (!token.HasValue) {
                TextSpan lineSpan = PositionHandler.CurrentPosition.Line > (PositionHandler.CurrentPosition.Parent?.Line ?? 1)
                    ? GetTextSpan(PositionHandler.CurrentPosition.Parent?.LineIndex ?? 0, (PositionHandler.CurrentPosition.Parent?.LineIndex ?? 0) + 2)
                    : GetTextSpan(PositionHandler.CurrentPosition.Parent?.LineIndex ?? 0, PositionHandler.CurrentPosition.LineIndex);


                syntaxErrors.Add(new DefaultSyntaxError(PositionHandler.CurrentPosition.Line,
                    GetTextSpan(PositionHandler.CurrentPosition.Parent?.Index ?? 0, PositionHandler.CurrentPosition.Index),
                    lineSpan,
                    PositionHandler.CurrentPosition.GetStringToParent(PositionHandler.Content)));
            }
            else
                tokens.Add(token.Value);
        }

        tokens.Add(new SyntaxToken("", SyntaxTokenKind.EndOfFile, new TextSpan(PositionHandler.CurrentPosition.Index, 0)));
        return [.. tokens];
    }

    public ImmutableArray<DefaultSyntaxError> GetSyntaxErrors() => syntaxErrors.ToImmutableArray();

    #region Lexing

    private SyntaxToken? GetNextToken() {
        // First check for null
        if (PositionHandler!.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.NULL)
            return null;

        // Check Whitespace
        if (PositionHandler.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.SPACE)
            return GetWhitespace();

        // Check NewLine
        if (PositionHandler.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.CR)
            return GetNewLine();

        // Check commands
        if (char.IsAsciiLetter(PositionHandler.CurrentPosition.GetValue(PositionHandler.Content)))
            return GetCommand();

        // Check numbers
        if (char.IsAsciiDigit(PositionHandler.CurrentPosition.GetValue(PositionHandler.Content)))
            return GetNumericLiteral();

        // Check single chars
        switch (PositionHandler.CurrentPosition.GetValue(PositionHandler.Content)) {
            case ';':
                return GetSemicolon();
            case '"':
                return GetStringLiteral();
            case '-':
                return GetCommandModifier();
        }

        return null;
    }

    private SyntaxToken GetWhitespace() {
        PositionHandler.MoveNextWhile(1, e => e.GetValue(PositionHandler.Content) == CommonCharCollection.SPACE);
        return new SyntaxToken(" ", SyntaxTokenKind.WhiteSpace, PositionHandler.CurrentPosition.GetSpanToParent());
    }
    private SyntaxToken GetNewLine() {
        PositionHandler.NewLine();
        return new SyntaxToken("\n\r", SyntaxTokenKind.EndOfLine, PositionHandler.CurrentPosition.GetSpanToParent());
    }
    private SyntaxToken GetSemicolon() {
        SyntaxToken token = new SyntaxToken(";", SyntaxTokenKind.Semicolon, new TextSpan(PositionHandler.CurrentPosition.Index, 1));
        PositionHandler.MoveNext(1);
        return token;
    }
    private SyntaxToken GetStringLiteral() {
        PositionHandler.MoveNextDoWhile(1, e => e.GetValue(PositionHandler.Content) != '"');
        PositionHandler.Skip(1); // Add second '"' to GetStringToParent value
        return new SyntaxToken(PositionHandler.CurrentPosition.GetStringToParent(PositionHandler.Content), SyntaxTokenKind.StringLiteral, PositionHandler.CurrentPosition.GetSpanToParent());
    }
    private SyntaxToken GetNumericLiteral() {
        PositionHandler.MoveNextWhile(1, e => char.IsAsciiDigit(e.GetValue(PositionHandler.Content)));
        return new SyntaxToken(PositionHandler.CurrentPosition.GetStringToParent(PositionHandler.Content), SyntaxTokenKind.NumericLiteral, PositionHandler.CurrentPosition.GetSpanToParent());
    }
    private SyntaxToken? GetCommand() {
        PositionHandler.MoveNextWhile(1, e => char.IsAsciiLetter(e.GetValue(PositionHandler.Content)));
        string value = PositionHandler.CurrentPosition.GetStringToParent(PositionHandler.Content);
        return value.ToLower() switch {
            "copy" => new SyntaxToken(value, SyntaxTokenKind.CopyKeyword, PositionHandler.CurrentPosition.GetSpanToParent()),
            "move" => new SyntaxToken(value, SyntaxTokenKind.MoveKeyword, PositionHandler.CurrentPosition.GetSpanToParent()),
            "replace" => new SyntaxToken(value, SyntaxTokenKind.ReplaceKeyword, PositionHandler.CurrentPosition.GetSpanToParent()),
            "to" => new SyntaxToken(value, SyntaxTokenKind.ToKeyword, PositionHandler.CurrentPosition.GetSpanToParent()),
            _ => null,
        };
    }
    private SyntaxToken? GetCommandModifier() {
        PositionHandler.MoveNextWhile(1, 1, e => char.IsAsciiLetter(e.GetValue(PositionHandler.Content)));
        string value = PositionHandler.CurrentPosition.GetStringToParent(PositionHandler.Content);

        switch (value.ToLower()) {
            case "-file":
            case "-f":
                return new SyntaxToken(value, SyntaxTokenKind.FileModifierKeyword, PositionHandler.CurrentPosition.GetSpanToParent());
            case "-directory":
            case "-dir":
            case "-d":
                return new SyntaxToken(value, SyntaxTokenKind.DirectoryModifierKeyword, PositionHandler.CurrentPosition.GetSpanToParent());
            default: 
                return null;
        }
    }
    #endregion

    #region Helper
    private static TextSpan GetTextSpan(int prevPosition, int currPosition) => new TextSpan(prevPosition, currPosition - prevPosition);
    #endregion
}
