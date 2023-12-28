using HB.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public class SyntaxTree(SyntaxNode root) : ISyntaxTree<SyntaxNode> {
    public SyntaxNode Root { get; } = root;
    public string? FilePath { get; internal set; }
}
