using FileManager.Core.Interpreter.Lexer;
using FileManager.Core.Interpreter.Parser;
using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter;
using HB.Code.Interpreter.Lexer;
using HB.Code.Interpreter.Lexer.Default;
using HB.Code.Interpreter.Parser;
using HB.Code.Interpreter.Syntax;
using System.Collections.Immutable;
using Unity;

namespace FileManager.Core.Interpreter;
public class FMInterpreter : IInterpreter {
    private readonly FMLexer lexer = new FMLexer();
    private readonly FMParser parser = new FMParser();

    public void Run(string input) {
        ImmutableArray<SyntaxToken> tokens = lexer!.Lex(input);
        ImmutableArray<DefaultSyntaxError> errors = lexer.GetSyntaxErrors();
        if (errors.Length > 0) {

            return;
        }


        SyntaxTree syntaxTree = parser!.Parse(tokens);


    }
}
