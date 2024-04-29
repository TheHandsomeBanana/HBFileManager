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

        Assert.AreEqual(26, foundTokens.Length);
        Assert.AreEqual(SyntaxTokenKind.CopyKeyword, foundTokens[0].Kind);
        Assert.AreEqual(SyntaxTokenKind.Semicolon, foundTokens[6].Kind);
        Assert.AreEqual(SyntaxTokenKind.Pipe, foundTokens[10].Kind);
        Assert.AreEqual(SyntaxTokenKind.OpenParenthesis, foundTokens[16].Kind);
        Assert.AreEqual(SyntaxTokenKind.EndOfFile, foundTokens[25].Kind);
    }

    [TestMethod]
    public void Lex_NegativeTest() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<SyntaxToken> foundTokens = lexer.Lex(LexerScriptError);
        ImmutableArray<SimpleError> foundSyntaxErrors = lexer.GetSyntaxErrors();
        Assert.AreEqual(9, foundTokens.Length);
        Assert.AreEqual(6, foundSyntaxErrors.Length);

        Assert.AreEqual("-", foundSyntaxErrors[0].Affected);
        Assert.AreEqual(new TextSpan(5, 1), foundSyntaxErrors[0].Location);
        Assert.AreEqual(new LineSpan(1, 5, 1), foundSyntaxErrors[0].LineSpan);

        Assert.AreEqual("ERROR", foundSyntaxErrors[5].Affected);
        Assert.AreEqual(new TextSpan(101, 5), foundSyntaxErrors[5].Location);
        Assert.AreEqual(new LineSpan(2, 44, 5), foundSyntaxErrors[5].LineSpan);
    }
}