using HBLibrary.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public class SyntaxTree(SyntaxNode root) : ISyntaxTree<SyntaxNode> {
    public SyntaxNode Root { get; } = root;
    public string? FilePath { get; internal set; }

    public SyntaxNode[] GetNodes() {
        return [.. GetNodesInternal(Root)];
    }

    private static List<SyntaxNode> GetNodesInternal(SyntaxNode current) {
        List<SyntaxNode> temp = [];

        foreach (SyntaxNode child in current.ChildNodes) {
            temp.Add(child);
            temp.AddRange(GetNodesInternal(child));
        }

        return temp;
    }
}
