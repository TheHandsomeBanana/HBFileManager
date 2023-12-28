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
        TextSpan[] spans = foundTokens.Select(e => e.FullSpan).ToArray();
        ImmutableArray<DefaultSyntaxError> foundSyntaxErrors = lexer.GetSyntaxErrors();
        Assert.AreEqual(0, foundSyntaxErrors.Length);
    }

    [TestMethod]
    public void Lex_NegativeTest() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<SyntaxToken> foundTokens = lexer.Lex(LexerScriptError);
        ImmutableArray<DefaultSyntaxError> foundSyntaxErrors = lexer.GetSyntaxErrors();

    }
}