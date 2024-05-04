using FileManager.Core.Interpreter.Lexer;
using FileManager.Core.Interpreter.Parser;
using FileManager.Core.Interpreter.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Tests;
[TestClass]
public class ParserTests : TestBase.TestBase {
    [TestMethod]
    public void Parse_PositiveTest() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<SyntaxToken> tokens = lexer.Lex(ParserScriptNoError);
        Assert.AreEqual(0, lexer.GetSyntaxErrors().Length);

        FMParser parser = new FMParser();
        SyntaxTree tree = parser.Parse(tokens);

        Assert.IsNotNull(tree);

        ImmutableArray<string> syntaxErrors = parser.GetSyntaxErrors(ParserScriptError)
            .Select(s => s.ToString()).ToImmutableArray();

        Assert.AreEqual(0, syntaxErrors.Length);

        SyntaxNode[] allNodes = tree.GetNodes();

        Assert.AreEqual(49, allNodes.Length);
    }

    [TestMethod]
    public void Parse_NegativeTest() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<SyntaxToken> tokens = lexer.Lex(ParserScriptError);
        Assert.AreEqual(0, lexer.GetSyntaxErrors().Length);

        FMParser parser = new FMParser();
        SyntaxTree tree = parser.Parse(tokens);

        ImmutableArray<string> syntaxErrors = parser.GetSyntaxErrors(ParserScriptError)
            .Select(s => s.ToString()).ToImmutableArray();


        Assert.IsNotNull(tree);
    }
}
