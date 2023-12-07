using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter;
using HB.Code.Interpreter.Lexer;
using HB.Code.Interpreter.Parser;
using System.Collections.Immutable;
using Unity;

namespace FileManager.Core.Interpreter;
public class FMInterpreter : IInterpreter {
    [Dependency]
    public ILexer<FMSyntaxToken>? Lexer { get; set; }
    [Dependency]
    public IParser<FMSyntaxTree, FMSyntaxToken>? Parser { get; set; }

    public void Run(string input) {
        ImmutableArray<FMSyntaxToken> tokens = Lexer!.Lex(input);
        FMSyntaxTree syntaxTree = Parser!.Parse(tokens);


    }
}
