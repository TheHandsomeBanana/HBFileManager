using FileManager.Core.Interpreter.Lexer;
using FileManager.Core.Interpreter.Syntax;
using FileManager.TestBase;
using HB.Code.Interpreter;
using HB.Code.Interpreter.Lexer;
using HB.Code.Interpreter.Lexer.Default;
using HB.Code.Interpreter.Syntax;
using System.Collections.Immutable;
using Unity;

namespace FileManager.Core.Interpreter.Tests;

[TestClass]
public class LexerTests : TestBase.TestBase {
    

    [TestMethod]
    public void Lex_PositiveTest() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<SyntaxToken> foundTokens = lexer.Lex(LexerScriptNoError);
        ImmutableArray<DefaultSyntaxError> foundSyntaxErrors = lexer.GetSyntaxErrors();
        Assert.AreEqual(0, foundSyntaxErrors.Length);

        Assert.AreEqual(15, foundTokens.Length);
        Assert.AreEqual(SyntaxTokenKind.CopyKeyword, foundTokens[0].Kind);
        Assert.AreEqual(SyntaxTokenKind.Semicolon, foundTokens[6].Kind);
        Assert.AreEqual(SyntaxTokenKind.EndOfFile, foundTokens[14].Kind);
    }

    [TestMethod]
    public void Lex_NegativeTest() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<SyntaxToken> foundTokens = lexer.Lex(LexerScriptError);
        ImmutableArray<DefaultSyntaxError> foundSyntaxErrors = lexer.GetSyntaxErrors();
        Assert.AreEqual(11, foundTokens.Length);
        Assert.AreEqual(6, foundSyntaxErrors.Length);

        Assert.AreEqual("-", foundSyntaxErrors[0].Affected);
        Assert.AreEqual(new TextSpan(5, 1), foundSyntaxErrors[0].FullSpan);
        Assert.AreEqual(new LineSpan(1, 5, 1), foundSyntaxErrors[0].LineSpan);

        Assert.AreEqual("ERROR", foundSyntaxErrors[5].Affected);
        Assert.AreEqual(new TextSpan(107, 5), foundSyntaxErrors[5].FullSpan);
        Assert.AreEqual(new LineSpan(2, 47, 5), foundSyntaxErrors[5].LineSpan);
    }
}