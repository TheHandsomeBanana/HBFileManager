using FileManager.Core.Interpreter.Lexer;
using FileManager.Core.Interpreter.Syntax;
using FileManager.TestBase;
using HBLibrary.Code.Interpreter;
using System.Collections.Immutable;
using Unity;

namespace FileManager.Core.Interpreter.Tests;

[TestClass]
public class LexerTests : TestBase.TestBase {
    

    [TestMethod]
    public void Lex_PositiveTest() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<SyntaxToken> foundTokens = lexer.Lex(LexerScriptNoError);
        ImmutableArray<SimpleError> foundSyntaxErrors = lexer.GetSyntaxErrors();
        Assert.AreEqual(0, foundSyntaxErrors.Length);

        Assert.AreEqual(41, foundTokens.Length);
        Assert.AreEqual(SyntaxTokenKind.CopyKeyword, foundTokens[0].Kind);
        Assert.AreEqual(SyntaxTokenKind.StringLiteral, foundTokens[6].Kind);
        Assert.AreEqual(SyntaxTokenKind.SourceParameter, foundTokens[10].Kind);
        Assert.AreEqual(SyntaxTokenKind.Comma, foundTokens[16].Kind);
        Assert.AreEqual(SyntaxTokenKind.EndOfFile, foundTokens[40].Kind);
    }

    [TestMethod]
    public void Lex_NegativeTest() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<SyntaxToken> foundTokens = lexer.Lex(LexerScriptError);
        ImmutableArray<SimpleError> foundSyntaxErrors = lexer.GetSyntaxErrors();
        Assert.AreEqual(7, foundTokens.Length);
        Assert.AreEqual(8, foundSyntaxErrors.Length);

        Assert.AreEqual("-", foundSyntaxErrors[0].Affected);
        Assert.AreEqual(new TextSpan(5, 1), foundSyntaxErrors[0].Span);
        Assert.AreEqual(new LineSpan(1, 0, 1, 5), foundSyntaxErrors[0].LineSpan);

        Assert.AreEqual("ERROR", foundSyntaxErrors[5].Affected);
        Assert.AreEqual(new TextSpan(101, 5), foundSyntaxErrors[5].Span);
        Assert.AreEqual(new LineSpan(5, 0, 2, 44), foundSyntaxErrors[5].LineSpan);
    }
}