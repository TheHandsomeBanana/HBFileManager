using FileManager.Core.Interpreter.Evaluator;
using FileManager.Core.Interpreter.Lexer;
using FileManager.Core.Interpreter.Parser;
using FileManager.Core.Interpreter.Syntax;
using FileManager.Core.Interpreter.Syntax.Commands;
using HBLibrary.Code.Interpreter;
using HBLibrary.Code.Interpreter.Evaluator;
using System.Collections.Immutable;

namespace FileManager.Core.Interpreter;
public class FMInterpreter : IInterpreter {
    private readonly static FMLexer lexer = new FMLexer();
    private readonly static FMParser parser = new FMParser();
    private readonly static FMEvaluator evaluator = new FMEvaluator();

    private IEnumerable<string> errors = Enumerable.Empty<string>();

    public void Run(string input) => RunInternal(input);

    public void RunFromFile(string filePath) {
        string input = File.ReadAllText(filePath);
        RunInternal(input, filePath);
    }
    public ImmutableArray<string> GetErrors() => errors.ToImmutableArray();

    private void RunInternal(string input, string? filePath = null) {
        ImmutableArray<SyntaxToken> tokens = lexer!.Lex(input);
        errors = lexer.GetSyntaxErrors().Select(e => e.ToString()!);
        if (errors.Any())
            return;

        SyntaxTree syntaxTree = parser!.Parse(tokens);
        syntaxTree.FilePath = filePath;
        errors = parser.GetSyntaxErrors().Select(e => {
            e.SetAffected(input);
            return e.ToString();
        });

        if (errors.Any())
            return;

        ImmutableArray<SimpleError> semanticErrors = evaluator.Evaluate(syntaxTree, input);
        errors = semanticErrors.Select(e => e.ToString()!);
        if (semanticErrors.Any())
            return;

        IReadOnlyList<SyntaxNode> commandNodes = syntaxTree.Root.ChildNodes;

        foreach (SyntaxNode node in commandNodes) {
            RunByNode(node);
        }
    }

    private void RunByNode(SyntaxNode node) {
        
    }

    private void RunCommand(CommandStatementSyntax commandExpression) {

    }
}
