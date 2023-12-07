using FileManager.Core.Interpreter.Lexer;
using FileManager.Core.Interpreter.Syntax;
using System.Collections.Immutable;

namespace FileManager.Core.Interpreter.Tests;

[TestClass]
public class LexerTests {
    private readonly string script = File.ReadAllText("../../../Assets/LexerScript_v1.txt");

    [TestInitialize]
    public void Initialize() {

    }

    [TestMethod]
    public void Lex_Positive() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<FMSyntaxToken> foundTokens = lexer.Lex(script);
    }
}