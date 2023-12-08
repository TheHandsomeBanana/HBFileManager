using FileManager.Core.Interpreter.Lexer;
using FileManager.Core.Interpreter.Syntax;
using FileManager.TestBase;
using HB.Code.Interpreter.Lexer;
using HB.Code.Interpreter.Location;
using System.Collections.Immutable;
using Unity;

namespace FileManager.Core.Interpreter.Tests;

[TestClass]
public class LexerTests : TestBase.TestBase {
    private readonly string scriptNoError = File.ReadAllText("../../../Assets/LexerScript_NoError.txt");
    private readonly string scriptError = File.ReadAllText("../../../Assets/LexerScript_Error.txt");

    [TestInitialize]
    public void Initialize() {

    }

    [TestMethod]
    public void Lex_Positive() {
        ILexer<FMSyntaxToken> lexer = UnityContainer.Resolve<ILexer<FMSyntaxToken>>();
        ImmutableArray<FMSyntaxToken> foundTokens = lexer.Lex(scriptNoError);
        TextSpan[] spans = foundTokens.Select(e => e.Span).ToArray();
        
    }

    [TestMethod]
    public void Lex_Negative() {
        ILexer<FMSyntaxToken> lexer = UnityContainer.Resolve<ILexer<FMSyntaxToken>>();
        ImmutableArray<FMSyntaxToken> foundTokens = lexer.Lex(scriptError);
    }
}