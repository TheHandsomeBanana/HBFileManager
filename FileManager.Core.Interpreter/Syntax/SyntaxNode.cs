

using HBLibrary.Code.Interpreter;
using HBLibrary.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public abstract class SyntaxNode : ISyntaxNode<SyntaxNode, SyntaxToken> {
    public TextSpan Span { get; set; }
    public LineSpan LineSpan { get; set; }
    public SyntaxNode? Parent { get; private set; }
    public SyntaxNodeKind Kind { get; }

    private readonly List<SyntaxNode> childNodes = [];
    public IReadOnlyList<SyntaxNode> ChildNodes => childNodes;

    private readonly List<SyntaxToken> childTokens = [];
    public IReadOnlyList<SyntaxToken> ChildTokens => childTokens;

    public SyntaxNode(SyntaxNodeKind kind)
    {
        Kind = kind;
    }

    public virtual void AddChildNode(SyntaxNode node)
    {
        node.Parent = this;
        childNodes.Add(node);
    }

    public virtual void AddChildToken(SyntaxToken token)
    {
        childTokens.Add(token);
    }

    public SyntaxNode GetRoot()
    {
        SyntaxNode temp = this;
        while (temp.Parent != null)
            temp = temp.Parent;

        return temp;
    }

    public bool IsKind(SyntaxNodeKind nodeKind) => Kind == nodeKind;

    public override string ToString()
    {
        return $"{Kind} {Span} {LineSpan}";
    }

    public SyntaxNode[] GetDescendantNodes() {
        return [.. GetDescendentNodesInternal(this)];
    }

    private static List<SyntaxNode> GetDescendentNodesInternal(SyntaxNode current) {
        List<SyntaxNode> temp = [];

        foreach (SyntaxNode child in current.ChildNodes) {
            temp.Add(child);
            temp.AddRange(GetDescendentNodesInternal(child));
        }

        return temp;
    }
}
