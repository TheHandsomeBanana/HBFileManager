using HB.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public class FMSyntaxTree(FMSyntaxNode root) : ISyntaxTree<FMSyntaxNode> {
    ISyntaxNode? ISyntaxTree.Root => Root;
    public FMSyntaxNode Root { get; } = root;
    public string? FilePath { get; internal set; }
}
