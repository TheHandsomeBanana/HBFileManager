using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter.Parser;
using System.Collections.Immutable;

namespace FileManager.Core.Interpreter.Parser;
public class FMParser : IParser<FMSyntaxTree, FMSyntaxToken> {
    public ParserFunctionList ParserFunctions { get; } = new ParserFunctionList();

    public void AddParserFunction<T>(IParserFunction<T> function) {
        ParserFunctions.AddOrUpdate(function);
    }

    public FMSyntaxTree Parse(ImmutableArray<FMSyntaxToken> tokens) {
        throw new NotImplementedException();
    }
}
