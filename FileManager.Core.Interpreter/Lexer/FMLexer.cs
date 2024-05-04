using FileManager.Core.Interpreter.Syntax;
using HBLibrary.Code.Interpreter;
using HBLibrary.Code.Interpreter.Lexer;
using System.Collections.Immutable;
using System.Text;

namespace FileManager.Core.Interpreter.Lexer;
public class FMLexer : ILexer<SyntaxToken> {
    private readonly List<SimpleError> syntaxErrors = [];
    private readonly LexContentReader ContentReader = new LexContentReader();
    private readonly List<SyntaxToken> tokens = [];
    public ImmutableArray<SyntaxToken> Lex(string input) {
        syntaxErrors.Clear();
        tokens.Clear();
        ContentReader.Init(input);

        ContentReader.ReadSingle();
        while (ContentReader.CurrentIndex < ContentReader.Content.Length) {
            SyntaxToken? token = GetNextToken();

            if (!token.HasValue) {
                // Skip last iteration syntax error --> would be empty
                if (ContentReader.CurrentIndex >= ContentReader.Content.Length)
                    continue;

                syntaxErrors.Add(new SimpleError(
                    ContentReader.GetSpan(),
                    ContentReader.GetLineSpan(),
                    "Invalid input",
                    ContentReader.GetString()));
            }
            else
                tokens.Add(token.Value);
        }

        tokens.Add(new SyntaxToken("",
            SyntaxTokenKind.EndOfFile,
            new TextSpan(ContentReader.CurrentIndex, 0),
            new LineSpan(ContentReader.CurrentLine, 0, ContentReader.CurrentLineIndex, 0)));

        return [.. tokens];
    }

    public ImmutableArray<SimpleError> GetSyntaxErrors() => syntaxErrors.ToImmutableArray();

    #region Lexing
    private SyntaxToken? GetNextToken() {
        // First check for null
        if (ContentReader.GetChar() == CommonCharCollection.NULL)
            return null;

        char currentValue = ContentReader.GetChar();
        while (currentValue == CommonCharCollection.SPACE || currentValue == CommonCharCollection.TAB || currentValue == CommonCharCollection.CR) {
            // Check Whitespace
            if (ContentReader.CanRead() && ContentReader.GetChar() == CommonCharCollection.SPACE)
                AddWhitespaceTrivia();

            // Check Tab
            if (ContentReader.CanRead() && ContentReader.GetChar() == CommonCharCollection.TAB)
                AddTabTrivia();

            // Check NewLine
            while (ContentReader.CanRead() && ContentReader.GetChar() == CommonCharCollection.CR)
                AddNewLineTrivia();

            currentValue = ContentReader.CanRead() ? ContentReader.GetChar() : CommonCharCollection.NULL;
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

        ContentReader.ReadSingle();
        return null;
    }

    private SyntaxToken? GetEquals() {
        SyntaxToken token = new SyntaxToken("=",
            SyntaxTokenKind.Equals,
            new TextSpan(ContentReader.CurrentIndex, 1),
            new LineSpan(ContentReader.CurrentLine, 0, ContentReader.CurrentLineIndex, 1));

        ContentReader.ReadSingle();
        return token;
    }

    private SyntaxToken? GetPipe() {
        SyntaxToken token = new SyntaxToken("|",
            SyntaxTokenKind.Pipe,
            new TextSpan(ContentReader.CurrentIndex, 1),
            new LineSpan(ContentReader.CurrentLine, 0, ContentReader.CurrentLineIndex, 1));

        ContentReader.ReadSingle();
        return token;
    }

    private SyntaxToken GetComma() {
        SyntaxToken token = new SyntaxToken(",",
            SyntaxTokenKind.Comma,
            new TextSpan(ContentReader.CurrentIndex, 1),
            new LineSpan(ContentReader.CurrentLine, 0, ContentReader.CurrentLineIndex, 1));

        ContentReader.ReadSingle();
        return token;
    }

    private SyntaxToken GetOpenBrace() {
        SyntaxToken token = new SyntaxToken("{",
            SyntaxTokenKind.OpenBrace,
            new TextSpan(ContentReader.CurrentIndex, 1),
            new LineSpan(ContentReader.CurrentLine, 0, ContentReader.CurrentLineIndex, 1));

        ContentReader.ReadSingle();
        return token;
    }
    private SyntaxToken GetCloseBrace() {
        SyntaxToken token = new SyntaxToken("}",
            SyntaxTokenKind.CloseBrace,
            new TextSpan(ContentReader.CurrentIndex, 1),
            new LineSpan(ContentReader.CurrentLine, 0, ContentReader.CurrentLineIndex, 1));

        ContentReader.ReadSingle();
        return token;
    }
    private SyntaxToken GetOpenParen() {
        SyntaxToken token = new SyntaxToken("(",
            SyntaxTokenKind.OpenParenthesis,
            new TextSpan(ContentReader.CurrentIndex, 1),
            new LineSpan(ContentReader.CurrentLine, 0, ContentReader.CurrentLineIndex, 1));

        ContentReader.ReadSingle();
        return token;
    }
    private SyntaxToken GetCloseParen() {
        SyntaxToken token = new SyntaxToken(")",
            SyntaxTokenKind.CloseParenthesis,
            new TextSpan(ContentReader.CurrentIndex, 1),
            new LineSpan(ContentReader.CurrentLine, 0, ContentReader.CurrentLineIndex, 1));

        ContentReader.ReadSingle();
        return token;
    }
    private void AddWhitespaceTrivia() {
        ContentReader.ReadWhile(() => ContentReader.GetChar() == CommonCharCollection.SPACE);
        SyntaxToken? last = tokens.LastOrDefault();
        if (!last.HasValue)
            return;

        last.Value.AddSyntaxTrivia(new SyntaxTrivia(false, SyntaxTriviaKind.WhiteSpace, ContentReader.GetSpan()));
    }
    private void AddTabTrivia() {
        ContentReader.ReadWhile(() => ContentReader.GetChar() == CommonCharCollection.TAB);
        SyntaxToken? last = tokens.LastOrDefault();
        if (!last.HasValue)
            return;

        last.Value.AddSyntaxTrivia(new SyntaxTrivia(false, SyntaxTriviaKind.Tab, ContentReader.GetSpan()));
    }
    private void AddNewLineTrivia() {
        ContentReader.ReadSingle();
        ContentReader.SkipSingle();
        SyntaxToken? last = tokens.LastOrDefault();
        if (!last.HasValue)
            return;

        last.Value.AddSyntaxTrivia(new SyntaxTrivia(false, SyntaxTriviaKind.EndOfLine, ContentReader.GetSpan()));
    }
    private SyntaxToken GetSemicolon() {
        SyntaxToken token = new SyntaxToken(";",
            SyntaxTokenKind.Semicolon,
            new TextSpan(ContentReader.CurrentIndex, 1),
            new LineSpan(ContentReader.CurrentLine, 0, ContentReader.CurrentLineIndex, 1));

        ContentReader.ReadSingle();
        return token;
    }
    private SyntaxToken GetStringLiteral() {
        ContentReader.ReadWhile(() => ContentReader.GetChar() != '"', 1, 1);
        return new SyntaxToken(ContentReader.GetString(),
            SyntaxTokenKind.StringLiteral,
            ContentReader.GetSpan(),
            ContentReader.GetLineSpan());
    }
    private SyntaxToken GetNumericLiteral() {
        ContentReader.ReadWhile(() => char.IsAsciiDigit(ContentReader.GetChar()));
        return new SyntaxToken(ContentReader.GetString(),
            SyntaxTokenKind.StringLiteral,
            ContentReader.GetSpan(),
            ContentReader.GetLineSpan());
    }
    private SyntaxToken? GetCommand() {
        string value = ContentReader.ReadWhile(() => char.IsAsciiLetter(ContentReader.GetChar()));
        return value.ToLower() switch {
            "copy" => new SyntaxToken(value,
                SyntaxTokenKind.CopyKeyword,
                ContentReader.GetSpan(),
                ContentReader.GetLineSpan()),
            "move" => new SyntaxToken(value,
                SyntaxTokenKind.MoveKeyword,
                ContentReader.GetSpan(),
                ContentReader.GetLineSpan()),
            "replace" => new SyntaxToken(value,
                SyntaxTokenKind.ReplaceKeyword,
                ContentReader.GetSpan(),
                ContentReader.GetLineSpan()),
            "archive" => new SyntaxToken(value,
                SyntaxTokenKind.ArchiveKeyword,
                ContentReader.GetSpan(),
                ContentReader.GetLineSpan()),
            _ => null,
        };
    }
    private SyntaxToken? GetCommandModifier() {
        string value = ContentReader.ReadWhile(() => char.IsAsciiLetter(ContentReader.GetChar()), 1);

        return value.ToLower() switch {
            "-s" or
            "-source" => (SyntaxToken?)new SyntaxToken(value,
                                SyntaxTokenKind.SourceParameter,
                                ContentReader.GetSpan(),
                                ContentReader.GetLineSpan()),
            "-t" or
            "-target" => (SyntaxToken?)new SyntaxToken(value,
                                SyntaxTokenKind.TargetParameter,
                                ContentReader.GetSpan(),
                                ContentReader.GetLineSpan()),
            "-mo" => (SyntaxToken?)new SyntaxToken(value,
                                SyntaxTokenKind.ModifiedOnlyParameter,
                                ContentReader.GetSpan(),
                                ContentReader.GetLineSpan()),
            "-type" => (SyntaxToken?)new SyntaxToken(value,
                                SyntaxTokenKind.TypeParameter,
                                ContentReader.GetSpan(),
                                ContentReader.GetLineSpan()),
            _ => null,
        };
    }
    #endregion
}
