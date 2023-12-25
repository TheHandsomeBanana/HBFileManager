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
    public void Parse_Positive() {
        FMLexer lexer = new FMLexer();
        ImmutableArray<SyntaxToken> tokens = lexer.Lex(ScriptNoError);

        FMParser parser = new FMParser();
        parser.Parse(tokens);
        
    }
}
