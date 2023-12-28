using HB.Code.Interpreter;
using HB.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public abstract class SyntaxNode : ISyntaxNode<SyntaxNode, SyntaxToken> {
    public TextSpan Span { get; set; }
    public SyntaxNode? Parent { get; private set; }
    public SyntaxNodeKind Kind { get; }

    private readonly List<SyntaxNode> childNodes = [];
    public IReadOnlyList<SyntaxNode> ChildNodes => childNodes;

    private readonly List<SyntaxToken> childTokens = [];
    public IReadOnlyList<SyntaxToken> ChildTokens => childTokens;

    public SyntaxNode(SyntaxNodeKind kind) {
        this.Kind = kind;
    }

    public virtual void AddChildNode(SyntaxNode node) {
        node.Parent = this;
        childNodes.Add(node);
    }

    public virtual void AddChildToken(SyntaxToken token) {
        childTokens.Add(token);
    }

    public SyntaxNode GetRoot() {
        SyntaxNode temp = this;
        while (temp.Parent != null)
            temp = temp.Parent;

        return temp;
    }

    public bool IsKind(SyntaxNodeKind nodeKind) => this.Kind == nodeKind;

    public override string ToString() {
        return $"{Kind} {Span}";
    }
}
