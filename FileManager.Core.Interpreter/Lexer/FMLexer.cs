using FileManager.Core.Interpreter.Syntax;
using HBLibrary.Code.Interpreter;
using HBLibrary.Code.Interpreter.Lexer;
using System.Collections.Immutable;
using System.Text;

namespace FileManager.Core.Interpreter.Lexer;
public class FMLexer : ILexer<SyntaxToken> {
    private readonly List<SimpleError> syntaxErrors = [];
    private readonly PositionReader PositionReader = new PositionReader();
    private readonly List<SyntaxToken> tokens = [];
    public ImmutableArray<SyntaxToken> Lex(string input) {
        syntaxErrors.Clear();
        tokens.Clear();
        PositionReader.Init(input);

        PositionReader.ReadSingle();
        while (PositionReader.CurrentIndex < PositionReader.Content.Length) {
            SyntaxToken? token = GetNextToken();

            if (!token.HasValue) {
                // Skip last iteration syntax error --> would be empty
                if (PositionReader.CurrentIndex >= PositionReader.Content.Length)
                    continue;

                syntaxErrors.Add(new SimpleError(
                    PositionReader.GetSpan(),
                    PositionReader.GetLineSpan(),
                    PositionReader.GetString(),
                    "Invalid input"));
            }
            else
                tokens.Add(token.Value);
        }

        tokens.Add(new SyntaxToken("",
            SyntaxTokenKind.EndOfFile,
            new TextSpan(PositionReader.CurrentIndex, 0),
            new LineSpan(PositionReader.CurrentLine, PositionReader.CurrentLineIndex, 0)));

        return [.. tokens];
    }

    public ImmutableArray<SimpleError> GetSyntaxErrors() => syntaxErrors.ToImmutableArray();

    #region Lexing

    private SyntaxToken? GetNextToken() {
        // First check for null
        if (PositionReader.GetChar() == CommonCharCollection.NULL)
            return null;

        char currentValue = PositionReader.GetChar();
        while (currentValue == CommonCharCollection.SPACE || currentValue == CommonCharCollection.TAB || currentValue == CommonCharCollection.CR) {
            // Check Whitespace
            if (PositionReader.GetChar() == CommonCharCollection.SPACE)
                AddWhitespaceTrivia();

            // Check Tab
            if (PositionReader.GetChar() == CommonCharCollection.TAB)
                AddTabTrivia();

            // Check NewLine
            while (PositionReader.GetChar() == CommonCharCollection.CR)
                AddNewLineTrivia();

            currentValue = PositionReader.GetChar();
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

        PositionReader.ReadSingle();
        return null;
    }

    private SyntaxToken? GetEquals() {
        SyntaxToken token = new SyntaxToken("=",
                                  SyntaxTokenKind.Equals,
                                  new TextSpan(PositionReader.CurrentIndex, 1),
                                  new LineSpan(PositionReader.CurrentLine, PositionReader.CurrentLineIndex, 1));

        PositionReader.ReadSingle();
        return token;
    }

    private SyntaxToken? GetPipe() {
        SyntaxToken token = new SyntaxToken("|",
                           SyntaxTokenKind.Pipe,
                           new TextSpan(PositionReader.CurrentIndex, 1),
                           new LineSpan(PositionReader.CurrentLine, PositionReader.CurrentLineIndex, 1));

        PositionReader.ReadSingle();
        return token;
    }

    private SyntaxToken GetComma() {
        SyntaxToken token = new SyntaxToken(",",
                    SyntaxTokenKind.Comma,
                    new TextSpan(PositionReader.CurrentIndex, 1),
                    new LineSpan(PositionReader.CurrentLine, PositionReader.CurrentLineIndex, 1));

        PositionReader.ReadSingle();
        return token;
    }

    private SyntaxToken GetOpenBrace() {
        SyntaxToken token = new SyntaxToken("{",
            SyntaxTokenKind.OpenBrace,
            new TextSpan(PositionReader.CurrentIndex, 1),
            new LineSpan(PositionReader.CurrentLine, PositionReader.CurrentLineIndex, 1));

        PositionReader.ReadSingle();
        return token;
    }
    private SyntaxToken GetCloseBrace() {
        SyntaxToken token = new SyntaxToken("}",
            SyntaxTokenKind.CloseBrace,
            new TextSpan(PositionReader.CurrentIndex, 1),
            new LineSpan(PositionReader.CurrentLine, PositionReader.CurrentLineIndex, 1));

        PositionReader.ReadSingle();
        return token;
    }
    private SyntaxToken GetOpenParen() {
        SyntaxToken token = new SyntaxToken("(",
            SyntaxTokenKind.OpenParenthesis,
            new TextSpan(PositionReader.CurrentIndex, 1),
            new LineSpan(PositionReader.CurrentLine, PositionReader.CurrentLineIndex, 1));

        PositionReader.ReadSingle();
        return token;
    }
    private SyntaxToken GetCloseParen() {
        SyntaxToken token = new SyntaxToken(")",
            SyntaxTokenKind.CloseParenthesis,
            new TextSpan(PositionReader.CurrentIndex, 1),
            new LineSpan(PositionReader.CurrentLine, PositionReader.CurrentLineIndex, 1));

        PositionReader.ReadSingle();
        return token;
    }
    private void AddWhitespaceTrivia() {
        PositionReader.ReadWhile(() => PositionReader.GetChar() == CommonCharCollection.SPACE);
        SyntaxToken? last = tokens.LastOrDefault();
        if (!last.HasValue)
            return;

        last.Value.AddSyntaxTrivia(new SyntaxTrivia(false, SyntaxTriviaKind.WhiteSpace, PositionReader.GetSpan()));
    }
    private void AddTabTrivia() {
        PositionReader.ReadWhile(() => PositionReader.GetChar() == CommonCharCollection.TAB);
        SyntaxToken? last = tokens.LastOrDefault();
        if (!last.HasValue)
            return;

        last.Value.AddSyntaxTrivia(new SyntaxTrivia(false, SyntaxTriviaKind.Tab, PositionReader.GetSpan()));
    }
    private void AddNewLineTrivia() {
        PositionReader.ReadSingle();
        SyntaxToken? last = tokens.LastOrDefault();
        if (!last.HasValue)
            return;

        last.Value.AddSyntaxTrivia(new SyntaxTrivia(false, SyntaxTriviaKind.EndOfLine, PositionReader.GetSpan()));
    }
    private SyntaxToken GetSemicolon() {
        SyntaxToken token = new SyntaxToken(";",
            SyntaxTokenKind.Semicolon,
            new TextSpan(PositionReader.CurrentIndex, 1),
            new LineSpan(PositionReader.CurrentLine, PositionReader.CurrentLineIndex, 1));

        PositionReader.ReadSingle();
        return token;
    }
    private SyntaxToken GetStringLiteral() {
        PositionReader.ReadWhile(() => PositionReader.GetChar() != '"');
        return new SyntaxToken(PositionReader.GetString(),
            SyntaxTokenKind.StringLiteral,
            PositionReader.GetSpan(),
            PositionReader.GetLineSpan());
    }
    private SyntaxToken GetNumericLiteral() {
        PositionReader.ReadWhile(() => char.IsAsciiDigit(PositionReader.GetChar()));
        return new SyntaxToken(PositionReader.GetString(),
            SyntaxTokenKind.StringLiteral,
            PositionReader.GetSpan(),
            PositionReader.GetLineSpan());
    }
    private SyntaxToken? GetCommand() {
        PositionReader.ReadWhile(() => char.IsAsciiLetter(PositionReader.GetChar()));
        string value = PositionReader.GetString();
        return value.ToLower() switch {
            "copy" => new SyntaxToken(value,
                SyntaxTokenKind.CopyKeyword,
                PositionReader.GetSpan(),
                PositionReader.GetLineSpan()),
            "move" => new SyntaxToken(value,
                SyntaxTokenKind.MoveKeyword,
                PositionReader.GetSpan(),
                PositionReader.GetLineSpan()),
            "replace" => new SyntaxToken(value,
                SyntaxTokenKind.ReplaceKeyword,
                PositionReader.GetSpan(),
                PositionReader.GetLineSpan()),
            _ => null,
        };
    }
    private SyntaxToken? GetCommandModifier() {
        PositionReader.ReadWhile(() => char.IsAsciiLetter(PositionReader.GetChar()));
        string value = PositionReader.GetString();

        return value.ToLower() switch {
            "-s" or
            "-source" => (SyntaxToken?)new SyntaxToken(value,
                                SyntaxTokenKind.SourceParameter,
                                PositionReader.GetSpan(),
                                PositionReader.GetLineSpan()),
            "-t" or
            "-target" => (SyntaxToken?)new SyntaxToken(value,
                                SyntaxTokenKind.TargetParameter,
                                PositionReader.GetSpan(),
                                PositionReader.GetLineSpan()),
            "-mo" => (SyntaxToken?)new SyntaxToken(value,
                                SyntaxTokenKind.ModifiedOnlyParameter,
                                PositionReader.GetSpan(),
                                PositionReader.GetLineSpan()),
            _ => null,
        };
    }
    #endregion

    #region Helper
    private static TextSpan GetTextSpan(int prevPosition, int currPosition) => new TextSpan(prevPosition, currPosition - prevPosition);
    #endregion
}
