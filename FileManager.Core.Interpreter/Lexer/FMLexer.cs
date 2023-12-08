using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter.Exceptions;
using HB.Code.Interpreter.Lexer;
using HB.Code.Interpreter.Location;
using System.Collections.Immutable;
using System.Text;
using Unity;

namespace FileManager.Core.Interpreter.Lexer;
public class FMLexer : ILexer<FMSyntaxToken> {

    [Dependency]
    public IPositionHandler<FMPosition>? PositionHandler { get; set; }

    private FMPositionHandler InternalPositionHandler => (FMPositionHandler)PositionHandler!;
    public ImmutableArray<FMSyntaxToken> Lex(string input) {
        if (PositionHandler is null)
            return [];

        PositionHandler.Init(input);

        List<FMSyntaxToken> tokens = [];

        PositionHandler.MoveNext(1);
        while (PositionHandler.CurrentPosition.GetValue(PositionHandler.Content) != CommonCharCollection.NULL) {
            FMSyntaxToken token = GetNextToken() 
                ?? throw new LexerException($"Syntax error at [Line {PositionHandler.CurrentPosition.Line} " 
                + $"{GetTextSpan(PositionHandler.CurrentPosition.Previous?.LineIndex ?? 0, PositionHandler.CurrentPosition.LineIndex)}]: "
                + $"{PositionHandler.CurrentPosition.GetStringFromPrevious(PositionHandler.Content)}");
            tokens.Add(token);
        }

        tokens.Add(new FMSyntaxToken("EndOfFile", SyntaxTokenKind.EndOfFile, new TextSpan(PositionHandler.CurrentPosition.Index, 0)));
        return [.. tokens];
    }

    #region Lexing

    private FMSyntaxToken? GetNextToken() {
        // First check for null
        if(PositionHandler!.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.NULL) 
            return null;

        // Check Whitespace
        if(PositionHandler.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.SPACE) {
            PositionHandler.MoveNextWhile(1, e => e.GetValue(PositionHandler.Content) == CommonCharCollection.SPACE);
            return new FMSyntaxToken(" ", SyntaxTokenKind.WhiteSpace, PositionHandler.CurrentPosition.GetSpanFromPrevious());
        }

        // Check NewLine
        if (PositionHandler.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.CR) {
            PositionHandler.MoveNext(1);
            InternalPositionHandler.NewLine();
            FMSyntaxToken token = new FMSyntaxToken("\n\r", SyntaxTokenKind.EndOfLine, PositionHandler.CurrentPosition.GetSpanFromPrevious());
            PositionHandler.MoveNext(1);
            return token;
        }
        
        // Check commands
        if (char.IsAsciiLetter(PositionHandler.CurrentPosition.GetValue(PositionHandler.Content))) {
            PositionHandler.MoveNextWhile(1, e => char.IsAsciiLetter(e.GetValue(PositionHandler.Content)));

            return PositionHandler.CurrentPosition.GetStringFromPrevious(PositionHandler.Content) switch {
                "COPY" => (FMSyntaxToken?)new FMSyntaxToken("COPY", SyntaxTokenKind.CopyKeyword, PositionHandler.CurrentPosition.GetSpanFromPrevious()),
                "MOVE" => (FMSyntaxToken?)new FMSyntaxToken("MOVE", SyntaxTokenKind.MoveKeyword, PositionHandler.CurrentPosition.GetSpanFromPrevious()),
                "FROM" => (FMSyntaxToken?)new FMSyntaxToken("FROM", SyntaxTokenKind.FromKeyword, PositionHandler.CurrentPosition.GetSpanFromPrevious()),
                "TO" => (FMSyntaxToken?)new FMSyntaxToken("TO", SyntaxTokenKind.ToKeyword, PositionHandler.CurrentPosition.GetSpanFromPrevious()),
                "FILE" => (FMSyntaxToken?)new FMSyntaxToken("FILE", SyntaxTokenKind.FileKeyword, PositionHandler.CurrentPosition.GetSpanFromPrevious()),
                "DIR" => (FMSyntaxToken?)new FMSyntaxToken("DIR", SyntaxTokenKind.DirectoryKeyword, PositionHandler.CurrentPosition.GetSpanFromPrevious()),
                _ => null,
            };
        }

        // Check numbers
        if (char.IsAsciiDigit(PositionHandler.CurrentPosition.GetValue(PositionHandler.Content))) {
            PositionHandler.MoveNextWhile(1, e => char.IsAsciiDigit(e.GetValue(PositionHandler.Content)));
            if (PositionHandler.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.NULL)
                return null;

            return new FMSyntaxToken(PositionHandler.CurrentPosition.GetStringFromPrevious(PositionHandler.Content), SyntaxTokenKind.NumericLiteral, PositionHandler.CurrentPosition.GetSpanFromPrevious());
        }

        // Check single chars
        switch (PositionHandler.CurrentPosition.GetValue(PositionHandler.Content)) {
            case ';':
                FMSyntaxToken token = new FMSyntaxToken(";", SyntaxTokenKind.Semicolon, new TextSpan(PositionHandler.CurrentPosition.Index, 1));
                PositionHandler.MoveNext(1);
                return token;
            case '"':
                PositionHandler.Skip(1); // Skip first "
                PositionHandler.MoveNextWhile(1, e => e.GetValue(PositionHandler.Content) != '"');
                if (PositionHandler.CurrentPosition.GetValue(PositionHandler.Content) == CommonCharCollection.NULL)
                    return null;

                token = new FMSyntaxToken(PositionHandler.CurrentPosition.GetStringFromPrevious(PositionHandler.Content), SyntaxTokenKind.StringLiteral, PositionHandler.CurrentPosition.GetSpanFromPrevious());
                PositionHandler.Skip(1); // Skip second "
                return token;
        }

        return null;
    }
    #endregion

    #region Helper
    private static TextSpan GetTextSpan(int prevPosition, int currPosition) => new TextSpan(prevPosition, currPosition - prevPosition);
    #endregion
}
