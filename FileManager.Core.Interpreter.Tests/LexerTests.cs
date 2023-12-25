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
    private readonly string scriptNoError = File.ReadAllText("../../../Assets/LexerScript_NoError.txt");
    private readonly string scriptError = File.ReadAllText("../../../Assets/LexerScript_Error.txt");

    [TestMethod]
    public void Lex_Positive() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<FMSyntaxToken> foundTokens = lexer.Lex(scriptNoError);
        TextSpan[] spans = foundTokens.Select(e => e.Span).ToArray();
        ImmutableArray<DefaultSyntaxError> foundSyntaxErrors = lexer.GetSyntaxErrors();
        Assert.AreEqual(0, foundSyntaxErrors.Length);
     }

    [TestMethod]
    public void Lex_Negative() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<FMSyntaxToken> foundTokens = lexer.Lex(scriptError);
        ImmutableArray<DefaultSyntaxError> foundSyntaxErrors = lexer.GetSyntaxErrors();

    }
}