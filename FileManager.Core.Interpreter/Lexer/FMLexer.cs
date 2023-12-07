using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter.Exceptions;
using HB.Code.Interpreter.Lexer;
using HB.Code.Interpreter.Location;
using System.Collections.Immutable;
using System.Text;
using Unity;

namespace FileManager.Core.Interpreter.Lexer;
public class FMLexer : ILexer<FMSyntaxToken> {
    private string input = "";
    private int currentLineCount = 1;
    private int currentLinePosition = 0;
    private int previousLinePosition = 0;
    private int previousPosition = -1;
    private int currentPosition = -1;
    private string currentWord = "";
    private char previousChar = CommonCharCollection.NULL;
    private char currentChar = CommonCharCollection.NULL;

    public ImmutableArray<FMSyntaxToken> Lex(string input) {
        this.input = input;

        List<FMSyntaxToken> tokens = [];
        GetNextChar();
        while (currentChar != CommonCharCollection.NULL) {
            FMSyntaxToken token = GetNextToken() ?? throw new LexerException($"Syntax error at [Line: {currentLineCount} {GetTextSpan(previousLinePosition, currentLinePosition)}]");
            tokens.Add(token);
        }

        tokens.Add(new FMSyntaxToken("EndOfFile", SyntaxTokenKind.EndOfFile, new TextSpan(currentPosition, 0)));
        Reset();
        return [.. tokens];
    }

    #region Lexing

    private FMSyntaxToken? GetNextToken() {
        previousPosition = this.currentPosition;
        previousLinePosition = this.currentLinePosition;

        // Check Whitespace and CRLF
        while (currentChar == CommonCharCollection.SPACE)
            GetNextChar();

        if (previousChar == CommonCharCollection.SPACE) {
            previousChar = CommonCharCollection.NULL;
            return new FMSyntaxToken(" ", SyntaxTokenKind.WhiteSpace, GetTextSpan(previousPosition));
        }

        if (currentChar == CommonCharCollection.CR) {
            GetNextChar();
            FMSyntaxToken token = new FMSyntaxToken("\n\r", SyntaxTokenKind.EndOfLine, GetTextSpan(previousPosition));
            currentLineCount++;
            currentLinePosition = 0;
            GetNextChar();
            return token;

        }
        else if (char.IsAsciiLetter(currentChar)) {
            GetNextWord();
            switch (currentWord) {
                case "COPY": return new FMSyntaxToken("COPY", SyntaxTokenKind.CopyKeyword, GetTextSpan(previousPosition));
                case "MOVE": return new FMSyntaxToken("MOVE", SyntaxTokenKind.MoveKeyword, GetTextSpan(previousPosition));
                case "FROM": return new FMSyntaxToken("FROM", SyntaxTokenKind.FromKeyword, GetTextSpan(previousPosition));
                case "TO": return new FMSyntaxToken("TO", SyntaxTokenKind.ToKeyword, GetTextSpan(previousPosition));
                case "FILE": return new FMSyntaxToken("FILE", SyntaxTokenKind.FileKeyword, GetTextSpan(previousPosition));
                case "DIR": return new FMSyntaxToken("DIR", SyntaxTokenKind.DirectoryKeyword, GetTextSpan(previousPosition));

            }
        }
        else if (char.IsAsciiDigit(currentChar)) {
            string number = "";
            while (char.IsAsciiDigit(currentChar)) {
                if (currentChar == CommonCharCollection.NULL)
                    return null;

                number += currentChar;
                GetNextChar();
            }
            return new FMSyntaxToken(number, SyntaxTokenKind.NumericLiteral, GetTextSpan(previousPosition));
        }

        switch (currentChar) {
            case ';':
                FMSyntaxToken token = new FMSyntaxToken(";", SyntaxTokenKind.Semicolon, new TextSpan(currentPosition, 1));
                GetNextChar();
                return token;
            case '"':
                string stringLiteral = BuildStringLiteral();
                if (currentChar == CommonCharCollection.NULL)
                    return null;

                return new FMSyntaxToken(stringLiteral, SyntaxTokenKind.StringLiteral, GetTextSpan(previousPosition));
        }

        return null;
    }

    private string BuildStringLiteral() {
        StringBuilder sb = new StringBuilder();
        GetNextChar();
        while (currentChar != '"') {
            if (currentChar == CommonCharCollection.NULL)
                return "";

            sb.Append(currentChar);
            GetNextChar();
        }
        GetNextChar();

        return sb.ToString();
    }

    private void GetNextWord() {
        StringBuilder sb = new StringBuilder();
        while (char.IsAsciiLetter(currentChar)) {
            if (currentChar == CommonCharCollection.NULL)
                return;

            sb.Append(currentChar);
            GetNextChar();
        }

        currentWord = sb.ToString();
    }

    private void GetNextChar() {
        previousChar = currentChar;
        currentLinePosition++;
        currentPosition++;
        if (currentPosition >= input.Length)
            currentChar = CommonCharCollection.NULL; // Terminate loop
        else
            currentChar = input[currentPosition];
    }

    private void Reset() {
        currentPosition = -1;
        currentLineCount = 0;
        currentLinePosition = 0;
        input = "";
        currentChar = CommonCharCollection.NULL;
        previousChar = CommonCharCollection.NULL;
        currentWord = "";
    }
    #endregion

    #region Helper
    private TextSpan GetTextSpan(int prevPosition) => new TextSpan(prevPosition, currentPosition - prevPosition);
    private TextSpan GetTextSpan(int prevPosition, int currPosition) => new TextSpan(prevPosition, currPosition - prevPosition);
    #endregion
}
