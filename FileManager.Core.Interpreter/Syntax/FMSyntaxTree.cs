using HB.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public class FMSyntaxTree : ISyntaxTree<FMSyntaxNode> {
    ISyntaxNode? ISyntaxTree.Root => Root;
    public FMSyntaxNode? Root { get; }
    public string? FilePath { get; }

}
