using FileManager.Core.Interpreter.Lexer;
using FileManager.Core.Interpreter.Parser;
using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter.Lexer.Default;
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
    public void Parse_Positive() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<SyntaxToken> tokens = lexer.Lex(ParserScriptNoError);

        FMParser parser = new FMParser();
        SyntaxTree tree = parser.Parse(tokens);

        ImmutableArray<string> syntaxErrors = parser.GetSyntaxErrors().Select(e => {
            e.SetAffected(ParserScriptNoError);
            return e.ToString();
        }).ToImmutableArray();

        Assert.IsNotNull(tree);
    }
}
