using HB.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public class SyntaxTree(SyntaxNode root) : ISyntaxTree<SyntaxNode> {
    ISyntaxNode? ISyntaxTree.Root => Root;
    public SyntaxNode Root { get; } = root;
    public string? FilePath { get; internal set; }
}
