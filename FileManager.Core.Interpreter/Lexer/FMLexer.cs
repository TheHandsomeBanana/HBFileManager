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
    private readonly List<DefaultSyntaxError> syntaxErrors = [];
    private readonly DefaultPositionHandler PositionHandler = new DefaultPositionHandler();
    private readonly List<SyntaxToken> tokens = [];
    public ImmutableArray<SyntaxToken> Lex(string input) {
        syntaxErrors.Clear();
        tokens.Clear();
        PositionHandler.Init(input);

        PositionHandler.MoveNext(1);
        while (PositionHandler.CurrentPosition.Index < PositionHandler.Content.Length) {
            SyntaxToken? token = GetNextToken();

            if (!token.HasValue) {
                // Skip last iteration syntax error --> would be empty
                if (PositionHandler.CurrentPosition.Index >= PositionHandler.Content.Length)
                    continue;

                syntaxErrors.Add(new DefaultSyntaxError(
                    PositionHandler.CurrentPosition.GetSpanToParent(),
                    PositionHandler.CurrentPosition.GetLineSpanToParent(),
                    PositionHandler.CurrentPosition.GetStringToParent(PositionHandler.Content),
                    "Invalid input"));
            }
            else
                tokens.Add(token.Value);
        }

        tokens.Add(new SyntaxToken("",
            SyntaxTokenKind.EndOfFile,
            new TextSpan(PositionHandler.CurrentPosition.Index, 0),
            new LineSpan(PositionHandler.CurrentPosition.Line, PositionHandler.CurrentPosition.LineIndex, 0)));

        return [.. tokens];
    }

    public ImmutableArray<DefaultSyntaxError> GetSyntaxErrors() => syntaxErrors.ToImmutableArray();

    #region Lexing

    private SyntaxToken? GetNextToken() {
        // First check for null
        if (PositionHandler!.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.NULL)
            return null;

        char currentValue = PositionHandler.CurrentPosition.GetValue(PositionHandler.Content);
        while (currentValue == CommonCharCollection.SPACE || currentValue == CommonCharCollection.TAB || currentValue == CommonCharCollection.CR) {
            // Check Whitespace
            if (PositionHandler.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.SPACE)
                AddWhitespaceTrivia();

            // Check Tab
            if (PositionHandler.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.TAB)
                AddTabTrivia();

            // Check NewLine
            while (PositionHandler.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.CR)
                AddNewLineTrivia();

            currentValue = PositionHandler.CurrentPosition.GetValue(PositionHandler.Content);
        }

        // Check commands
        if (char.IsAsciiLetter(currentValue))
            return GetCommand();

        // Check numbers
        if (char.IsAsciiDigit(currentValue))
            return GetNumericLiteral();

        // Check single chars
        switch (currentValue) {
            case ';':
                return GetSemicolon();
            case ',':
                return GetComma();
            case '"':
                return GetStringLiteral();
            case '-':
                return GetCommandModifier();
            case '{':
                return GetOpenBrace();
            case '}':
                return GetCloseBrace();
            case '(':
                return GetOpenParen();
            case ')':
                return GetCloseParen();
            case '|':
                return GetPipe();
            case '=':
                return GetEquals();
        }




        PositionHandler.MoveNext(1);
        return null;
    }

    private SyntaxToken? GetEquals() {
        SyntaxToken token = new SyntaxToken("=",
                                  SyntaxTokenKind.Equals,
                                  new TextSpan(PositionHandler.CurrentPosition.Index, 1),
                                  new LineSpan(PositionHandler.CurrentPosition.Line, PositionHandler.CurrentPosition.LineIndex, 1));
        PositionHandler.MoveNext(1);
        return token;
    }

    private SyntaxToken? GetPipe() {
        SyntaxToken token = new SyntaxToken("|",
                           SyntaxTokenKind.Pipe,
                           new TextSpan(PositionHandler.CurrentPosition.Index, 1),
                           new LineSpan(PositionHandler.CurrentPosition.Line, PositionHandler.CurrentPosition.LineIndex, 1));
        PositionHandler.MoveNext(1);
        return token;
    }

    private SyntaxToken GetComma() {
        SyntaxToken token = new SyntaxToken(",",
                    SyntaxTokenKind.Comma,
                    new TextSpan(PositionHandler.CurrentPosition.Index, 1),
                    new LineSpan(PositionHandler.CurrentPosition.Line, PositionHandler.CurrentPosition.LineIndex, 1));
        PositionHandler.MoveNext(1);
        return token;
    }

    private SyntaxToken GetOpenBrace() {
        SyntaxToken token = new SyntaxToken("{",
            SyntaxTokenKind.OpenBrace,
            new TextSpan(PositionHandler.CurrentPosition.Index, 1),
            new LineSpan(PositionHandler.CurrentPosition.Line, PositionHandler.CurrentPosition.LineIndex, 1));
        PositionHandler.MoveNext(1);
        return token;
    }
    private SyntaxToken GetCloseBrace() {
        SyntaxToken token = new SyntaxToken("}",
            SyntaxTokenKind.CloseBrace,
            new TextSpan(PositionHandler.CurrentPosition.Index, 1),
            new LineSpan(PositionHandler.CurrentPosition.Line, PositionHandler.CurrentPosition.LineIndex, 1));
        PositionHandler.MoveNext(1);
        return token;
    }
    private SyntaxToken GetOpenParen() {
        SyntaxToken token = new SyntaxToken("(",
            SyntaxTokenKind.OpenParenthesis,
            new TextSpan(PositionHandler.CurrentPosition.Index, 1),
            new LineSpan(PositionHandler.CurrentPosition.Line, PositionHandler.CurrentPosition.LineIndex, 1));
        PositionHandler.MoveNext(1);
        return token;
    }
    private SyntaxToken GetCloseParen() {
        SyntaxToken token = new SyntaxToken(")",
            SyntaxTokenKind.CloseParenthesis,
            new TextSpan(PositionHandler.CurrentPosition.Index, 1),
            new LineSpan(PositionHandler.CurrentPosition.Line, PositionHandler.CurrentPosition.LineIndex, 1));
        PositionHandler.MoveNext(1);
        return token;
    }
    private void AddWhitespaceTrivia() {
        PositionHandler.MoveNextWhile(1, e => e.GetValue(PositionHandler.Content) == CommonCharCollection.SPACE);
        SyntaxToken? last = tokens.LastOrDefault();
        if (!last.HasValue)
            return;

        last.Value.AddSyntaxTrivia(new SyntaxTrivia(false, SyntaxTriviaKind.WhiteSpace, PositionHandler.CurrentPosition.GetSpanToParent()));
    }
    private void AddTabTrivia() {
        PositionHandler.MoveNextWhile(1, e => e.GetValue(PositionHandler.Content) == CommonCharCollection.TAB);
        SyntaxToken? last = tokens.LastOrDefault();
        if (!last.HasValue)
            return;

        last.Value.AddSyntaxTrivia(new SyntaxTrivia(false, SyntaxTriviaKind.Tab, PositionHandler.CurrentPosition.GetSpanToParent()));
    }
    private void AddNewLineTrivia() {
        PositionHandler.NewLine();
        SyntaxToken? last = tokens.LastOrDefault();
        if (!last.HasValue)
            return;

        last.Value.AddSyntaxTrivia(new SyntaxTrivia(false, SyntaxTriviaKind.EndOfLine, PositionHandler.CurrentPosition.GetSpanToParent()));
    }
    private SyntaxToken GetSemicolon() {
        SyntaxToken token = new SyntaxToken(";",
            SyntaxTokenKind.Semicolon,
            new TextSpan(PositionHandler.CurrentPosition.Index, 1),
            new LineSpan(PositionHandler.CurrentPosition.Line, PositionHandler.CurrentPosition.LineIndex, 1));

        PositionHandler.MoveNext(1);
        return token;
    }
    private SyntaxToken GetStringLiteral() {
        PositionHandler.MoveNextDoWhile(1, e => e.GetValue(PositionHandler.Content) != '"');
        PositionHandler.Skip(1); // Add second '"' to GetStringToParent value
        return new SyntaxToken(PositionHandler.CurrentPosition.GetStringToParent(PositionHandler.Content),
            SyntaxTokenKind.StringLiteral,
            PositionHandler.CurrentPosition.GetSpanToParent(),
            PositionHandler.CurrentPosition.GetLineSpanToParent());
    }
    private SyntaxToken GetNumericLiteral() {
        PositionHandler.MoveNextWhile(1, e => char.IsAsciiDigit(e.GetValue(PositionHandler.Content)));
        return new SyntaxToken(PositionHandler.CurrentPosition.GetStringToParent(PositionHandler.Content),
            SyntaxTokenKind.NumericLiteral,
            PositionHandler.CurrentPosition.GetSpanToParent(),
            PositionHandler.CurrentPosition.GetLineSpanToParent());
    }
    private SyntaxToken? GetCommand() {
        PositionHandler.MoveNextWhile(1, e => char.IsAsciiLetter(e.GetValue(PositionHandler.Content)));
        string value = PositionHandler.CurrentPosition.GetStringToParent(PositionHandler.Content);
        return value.ToLower() switch {
            "copy" => new SyntaxToken(value,
                SyntaxTokenKind.CopyKeyword,
                PositionHandler.CurrentPosition.GetSpanToParent(),
                PositionHandler.CurrentPosition.GetLineSpanToParent()),
            "move" => new SyntaxToken(value,
                SyntaxTokenKind.MoveKeyword,
                PositionHandler.CurrentPosition.GetSpanToParent(),
                PositionHandler.CurrentPosition.GetLineSpanToParent()),
            "replace" => new SyntaxToken(value,
                SyntaxTokenKind.ReplaceKeyword,
                PositionHandler.CurrentPosition.GetSpanToParent(),
                PositionHandler.CurrentPosition.GetLineSpanToParent()),
            _ => null,
        };
    }
    private SyntaxToken? GetCommandModifier() {
        PositionHandler.MoveNextWhile(1, 1, e => char.IsAsciiLetter(e.GetValue(PositionHandler.Content)));
        string value = PositionHandler.CurrentPosition.GetStringToParent(PositionHandler.Content);

        return value.ToLower() switch {
            "-s" or
            "-source" => (SyntaxToken?)new SyntaxToken(value,
                                SyntaxTokenKind.SourceParameter,
                                PositionHandler.CurrentPosition.GetSpanToParent(),
                                PositionHandler.CurrentPosition.GetLineSpanToParent()),
            "-t" or
            "-target" => (SyntaxToken?)new SyntaxToken(value,
                                SyntaxTokenKind.TargetParameter,
                                PositionHandler.CurrentPosition.GetSpanToParent(),
                                PositionHandler.CurrentPosition.GetLineSpanToParent()),
            "-mo" => (SyntaxToken?)new SyntaxToken(value,
                                SyntaxTokenKind.ModifiedOnlyParameter,
                                PositionHandler.CurrentPosition.GetSpanToParent(),
                                PositionHandler.CurrentPosition.GetLineSpanToParent()),
            _ => null,
        };
    }
    #endregion

    #region Helper
    private static TextSpan GetTextSpan(int prevPosition, int currPosition) => new TextSpan(prevPosition, currPosition - prevPosition);
    #endregion
}
